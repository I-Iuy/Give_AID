using Be.DTOs.Account;
using Be.Models;
using Be.Repositories.Accounts;
using Be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Be.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository _repository;
        private readonly JwtService _jwtService;

        public AccountsController(IAccountRepository repository, JwtService jwtService)
        {
            _repository = repository;
            _jwtService = jwtService;
        }

        // POST: api/accounts/register
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

        // POST: api/accounts/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(AccountLoginDto dto)
        {
            var account = await _repository.LoginAsync(dto.Email, dto.Password);
            if (account == null || !BCrypt.Net.BCrypt.Verify(dto.Password, account.Password))
                return Unauthorized("Invalid credentials or inactive account.");

            var token = _jwtService.GenerateToken(account);
            return Ok(new { token });
        }

        // GET: api/accounts/me
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

        // PUT: api/accounts/{id}/status?isActive=true
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromQuery] bool isActive)
        {
            var success = await _repository.SetAccountStatusAsync(id, isActive);
            if (!success) return NotFound();
            return Ok(new { message = $"Account {(isActive ? "activated" : "deactivated")} successfully." });
        }
    }
}
