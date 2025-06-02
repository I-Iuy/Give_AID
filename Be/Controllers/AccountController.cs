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

        //REGISTER

        [HttpPost("register")]
        public async Task<IActionResult> Register(AccountCreateDto dto)
        {
            var existing = await _repository.LoginAsync(dto.Email, dto.Password);
            if (existing != null)
                return BadRequest("Email already exists or is in use.");

            var account = new Account
            {
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                DisplayName = dto.DisplayName,
                Phone = dto.Phone,
                Address = dto.Address,
                Name = dto.Name,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role,
                IsActive = true
            };

            var result = await _repository.CreateAsync(account);
            return Ok(new { message = "Registration successful", result.AccountId });
        }

        //LOGIN
        // POST: api/accounts/login
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


        //SHOW ACCOUNT INFO
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
                Role = account.Role
            };

            return Ok(dto);
        }

        //SET ACTIVE AND DEACTIVATED WITH ACCOUNT = ADMIN
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromQuery] bool isActive)
        {
            var success = await _repository.SetAccountStatusAsync(id, isActive);
            if (!success) return NotFound();
            return Ok(new { message = $"Account {(isActive ? "activated" : "deactivated")} successfully." });
        }

        //GOOGLE LOGIN
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


        //Reset Password (User forgot password)

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
                    details = ex.ToString() // hoặc ex.InnerException?.Message nếu bạn chỉ cần chi tiết nhất
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

        //User Update Account Info
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

        //User Change Password when logging in
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int accountId)) return Unauthorized();

            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var result = await _repository.ChangePasswordAsync(accountId, dto.CurrentPassword, hashed);

            if (result == 0)
                return NotFound("Account not found or inactive.");

            if (result == 1)
                return BadRequest("Current password is incorrect.");

            return Ok(new { message = "Password changed successfully." });
        }

        //Admin See All Account List on Dashboard
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
