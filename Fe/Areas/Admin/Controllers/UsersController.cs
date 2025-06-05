// ==============================
// UsersController (Admin Area)
// Handles user list, detail, and status toggling for admin panel
// ==============================

using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

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
        public async Task<IActionResult> List(string? search, string? role, string sortBy = "FullName", bool desc = false)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { area = "Web" });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Build API URL with optional parameters
            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}accounts/all?sortBy={sortBy}&desc={desc.ToString().ToLower()}";

            if (!string.IsNullOrWhiteSpace(search))
                apiUrl += $"&search={Uri.EscapeDataString(search)}";
            if (!string.IsNullOrWhiteSpace(role))
                apiUrl += $"&role={Uri.EscapeDataString(role)}";

            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Failed to load user list.";
                TempData["MessageType"] = "danger";
                return View(new List<User>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Store filtering and sorting state in ViewBag
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.SortBy = sortBy;
            ViewBag.Desc = desc.ToString().ToLower();

            return View(users);
        }

        // Toggle user active status (block/unblock)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { area = "Web" });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}accounts/{id}/status?isActive={isActive}";
            var response = await client.PutAsync(apiUrl, null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = $"User has been {(isActive ? "activated" : "blocked")} successfully.";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Failed to update user status.";
                TempData["MessageType"] = "danger";
            }

            return RedirectToAction("List");
        }

        // Show user details by ID
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
