// ==============================
// UsersController (Admin Area)
// Handles user list, detail, and status toggling for admin panel
// ==============================

using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Be.DTOs.Account;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;

        public UsersController(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _config = config;
        }

        // Represents user summary data
        public class User
        {
            public int AccountId { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string DisplayName { get; set; }
            public string Role { get; set; }
            public bool IsActive { get; set; }
        }

        // Represents detailed user profile data
        public class UserDetail
        {
            public int AccountId { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string DisplayName { get; set; }
            public string Role { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public bool IsActive { get; set; }
        }

        // Display user list with filtering, sorting, and role search
        public async Task<IActionResult> List(string sortBy = "FullName", bool desc = false)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { area = "Web" });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiSettings:BaseUrl"]}accounts/all?sortBy={sortBy}&desc={desc}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Failed to retrieve user list.";
                TempData["MessageType"] = "danger";
                return View(new List<User>());
            }

            var result = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            
            var filteredUsers = users.Where(u => u.Role != "Admin").ToList();

            return View(filteredUsers);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive, string? reason)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { area = "Web" });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}accounts/{id}/status";

            var payload = new
            {
                isActive,
                reason = isActive ? "" : reason ?? "Blocked by admin."
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PutAsync(apiUrl, content);
            var errorDetail = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = $"User has been {(isActive ? "activated" : "blocked")} successfully.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = $"Failed to update user status. Error: {errorDetail}";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("List");
        }


        // Block user via PUT (JavaScript Fetch with reason)
        // PUT: Admin/Users/ToggleStatusWithReason (Block from Modal)
        [HttpPut]
        public async Task<IActionResult> ToggleStatusWithReason(int id, [FromBody] StatusUpdateViewModel dto)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}accounts/{id}/status";

            var content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json")
            );

            var response = await client.PutAsync(apiUrl, content);
            var errorDetail = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return Ok(new { message = "User blocked successfully." });
            else
                return BadRequest(new { message = "Failed to block user", detail = errorDetail });
        }


        // GET: Admin/Users/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { area = "Web" });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}accounts/{id}";
            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Failed to load user details.";
                TempData["MessageType"] = "danger";
                return RedirectToAction("List");
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDetail>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(user);
        }
    }
}
