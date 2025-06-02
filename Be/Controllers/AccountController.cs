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

namespace Be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository _repository;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AccountsController(IAccountRepository repository, JwtService jwtService, IConfiguration config, EmailService emailService)
        {
            _repository = repository;
            _jwtService = jwtService;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var shortErrors = ModelState.Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(err => err.ErrorMessage.Split('.').FirstOrDefault()).ToArray()
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
                Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim(),
                Role = "User",
                IsActive = true
            };

            var result = await _repository.CreateAsync(account);
            return Ok(new { message = "Registration successful", result.AccountId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AccountLoginDto dto)
        {
            var account = await _repository.LoginAsync(dto.Email, dto.Password);

            if (account == null)
                return Unauthorized("Invalid credentials or inactive account.");

            if (string.IsNullOrEmpty(account.Password))
                return Unauthorized("This email was registered using Google. Please log in using Google instead.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, account.Password))
                return Unauthorized("Invalid credentials or inactive account.");

            var token = _jwtService.GenerateToken(account);
            return Ok(new { token });
        }

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
                Address = account.Address
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromQuery] bool isActive)
        {
            var success = await _repository.SetAccountStatusAsync(id, isActive);
            if (!success) return NotFound();
            return Ok(new { message = $"Account {(isActive ? "activated" : "deactivated")} successfully." });
        }

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

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var success = await _repository.ResetPasswordAsync(dto.Token, hashed);
            if (!success) return BadRequest("Invalid or expired token.");

            return Ok(new { message = "Password reset successfully." });
        }

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

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int accountId)) return Unauthorized();

            var account = await _repository.GetByIdAsync(accountId);
            if (account == null || !account.IsActive)
                return NotFound("Account not found or inactive.");

            // ✅ Chặn Google account đổi mật khẩu
            if (string.IsNullOrEmpty(account.Password))
                return BadRequest("This account was registered using Google login. Please change your password via your Google account.");

            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var result = await _repository.ChangePasswordAsync(accountId, dto.CurrentPassword, hashed);

            if (result == 1)
                return BadRequest("Current password is incorrect.");

            return Ok(new { message = "Password changed successfully." });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _repository.GetAllAsync();

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
    }
}
