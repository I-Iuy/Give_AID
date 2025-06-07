// ========================
// AccountsController.cs
// ========================

using Be.DTOs.Account;
using Be.Models;
using Be.Repositories.Accounts;
using Be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Security;
using System.Security.Claims;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository _repository;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _config;
        private readonly EmailServices _emailService;

        public AccountsController(IAccountRepository repository, JwtService jwtService, IConfiguration config, EmailServices emailService)
        {
            _repository = repository;
            _jwtService = jwtService;
            _config = config;
            _emailService = emailService;
        }

        // -------------------------------
        // POST: api/accounts/register
        // Handles user registration
        // -------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var shortErrors = ModelState.Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(err => err.ErrorMessage).ToArray()
                    );
                return BadRequest(new { message = "Validation failed", errors = shortErrors });
            }

            var existing = await _repository.GetByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest(new { errors = new { Email = new[] { "Email already exists or in use." } } });

            var account = new Account
            {
                Email = dto.Email.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName.Trim(),
                DisplayName = dto.DisplayName.Trim(),
                Phone = dto.Phone.Trim(),
                Address = dto.Address.Trim(),
                Role = "User",
                IsActive = true
            };

            var result = await _repository.CreateAsync(account);
            return Ok(new { message = "Registration successful", result.AccountId });
        }

        // -------------------------------
        // POST: api/accounts/login
        // Handles user login
        // -------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(AccountLoginDto dto)
        {
            var account = await _repository.LoginAsync(dto.Email, dto.Password);

            if (account == null)
                return Unauthorized("Invalid credentials or inactive account.");

            if (!account.IsActive)
                return Unauthorized("Your account has been blocked. Please contact support.");

            if (string.IsNullOrEmpty(account.Password))
                return Unauthorized("This email was registered using Google. Please log in using Google instead.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, account.Password))
                return Unauthorized("Invalid credentials or inactive account.");

            var token = _jwtService.GenerateToken(account);
            return Ok(new { token });
        }

        // -------------------------------
        // GET: api/accounts/me
        // Returns profile of currently logged-in user
        // -------------------------------
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyInfo()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int userId)) return Unauthorized();

            var account = await _repository.GetByIdAsync(userId);
            if (account == null) return NotFound();

            var dto = new AccountGetDto
            {
                AccountId = account.AccountId,
                Email = account.Email,
                FullName = account.FullName,
                DisplayName = account.DisplayName,
                Role = account.Role,
                Phone = account.Phone,
                Address = account.Address,
                IsActive = account.IsActive
            };

            return Ok(dto);
        }

        // -------------------------------
        // PUT: api/accounts/{id}/status
        // Admin-only: activates or deactivates a user
        // -------------------------------
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var success = await _repository.SetAccountStatusAsync(id, dto.IsActive);
            if (!success) return NotFound();

            if (!dto.IsActive)
            {
                var account = await _repository.GetByIdAsync(id);
                if (account != null)
                {
                    await _emailService.SendCustomEmail(
                        account.Email,
                        "Your account has been blocked",
                        $"Dear {account.FullName ?? account.Email},\n\nYour account has been blocked for the following reason:\n\n{dto.Reason}\n\nIf you believe this is a mistake, please contact support."
                    );
                }
            }

            return Ok(new { message = $"Account {(dto.IsActive ? "activated" : "deactivated")} successfully." });
        }




        // -------------------------------
        // POST: api/accounts/google-login
        // Handles Google-based login
        // -------------------------------
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var httpClient = new HttpClient(handler);
            var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={dto.IdToken}");

            if (!response.IsSuccessStatusCode)
                return Unauthorized("Invalid Google token.");

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            var email = data["email"];
            var name = data.ContainsKey("name") ? data["name"] : "";
            var givenName = data.ContainsKey("given_name") ? data["given_name"] : "";

            var account = await _repository.LoginAsync(email, "");
            if (account == null)
            {
                account = new Account
                {
                    Email = email,
                    FullName = name,
                    DisplayName = givenName,
                    Role = "User",
                    IsActive = true,
                    Password = ""
                };

                await _repository.CreateAsync(account);
            }

            var token = _jwtService.GenerateToken(account);
            return Ok(new { token });
        }

        // -------------------------------
        // POST: api/accounts/forgot-password
        // Sends a reset link to the user's email
        // -------------------------------
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var token = Guid.NewGuid().ToString();
            var expires = DateTime.UtcNow.AddMinutes(15);

            var success = await _repository.SetResetTokenAsync(dto.Email, token, expires);
            if (!success) return NotFound("Email not found.");

            var resetLink = $"https://localhost:7108/web/account/resetpassword?token={token}";

            try
            {
                await _emailService.SendResetEmail(dto.Email, resetLink);
                return Ok(new { message = "A password reset link has been sent to your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Unable to send reset email. Please check your email configuration.",
                    details = ex.ToString()
                });
            }
        }

        // -------------------------------
        // POST: api/accounts/reset-password
        // Resets user's password using token
        // -------------------------------
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var success = await _repository.ResetPasswordAsync(dto.Token, hashed);
            if (!success) return BadRequest("Invalid or expired token.");

            return Ok(new { message = "Password reset successfully." });
        }

        // -------------------------------
        // PUT: api/accounts/update
        // Updates user profile
        // -------------------------------
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] AccountUpdateDto dto)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int accountId)) return Unauthorized();

            var success = await _repository.UpdateAccountInfoAsync(accountId, dto);
            if (!success) return BadRequest("Account not found or inactive.");

            return Ok(new { message = "Profile updated successfully." });
        }

        // -------------------------------
        // PUT: api/accounts/change-password
        // Allows user to change password
        // -------------------------------
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int accountId)) return Unauthorized();

            var account = await _repository.GetByIdAsync(accountId);
            if (account == null || !account.IsActive)
                return NotFound("Account not found or inactive.");

            if (string.IsNullOrEmpty(account.Password))
                return BadRequest("This account was registered using Google login. Please change your password via your Google account.");

            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var result = await _repository.ChangePasswordAsync(accountId, dto.CurrentPassword, hashed);

            if (result == 1)
                return BadRequest("Current password is incorrect.");

            return Ok(new { message = "Password changed successfully." });
        }

        // -------------------------------
        // GET: api/accounts/all
        // Admin-only: gets list of all accounts with filtering/sorting
        // -------------------------------
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAccounts([FromQuery] string? search = null, [FromQuery] string? role = null, [FromQuery] string sortBy = "FullName", [FromQuery] bool desc = false)
        {
            var accounts = await _repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                accounts = accounts.Where(a =>
                    a.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    a.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    a.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                accounts = accounts.Where(a => a.Role.Equals(role, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                try
                {
                    accounts = desc
                        ? accounts.AsQueryable().OrderBy($"{sortBy} descending").ToList()
                        : accounts.AsQueryable().OrderBy($"{sortBy}").ToList();
                }
                catch
                {
                    return BadRequest("Invalid sort field.");
                }
            }

            var result = accounts.Select(a => new AccountListItemDto
            {
                AccountId = a.AccountId,
                Email = a.Email,
                FullName = a.FullName,
                DisplayName = a.DisplayName,
                Role = a.Role,
                IsActive = a.IsActive
            });

            return Ok(result);
        }

        // -------------------------------
        // GET: api/accounts/{id}
        // Admin-only: get single account by ID
        // -------------------------------
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var account = await _repository.GetByIdAsync(id);
            if (account == null)
                return NotFound();

            var dto = new AccountGetDto
            {
                AccountId = account.AccountId,
                Email = account.Email,
                FullName = account.FullName,
                DisplayName = account.DisplayName,
                Role = account.Role,
                Phone = account.Phone,
                Address = account.Address,
                IsActive = account.IsActive
            };

            return Ok(dto);
        }
    }
}
