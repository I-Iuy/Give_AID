using Fe.DTOs.Notification;
using Fe.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Fe.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public NotificationController(
            INotificationService notificationService,
            IHttpClientFactory clientFactory,
            IConfiguration config)
        {
            _notificationService = notificationService;
            _httpClient = clientFactory.CreateClient();
            _config = config;
        }

        // ✅ 1. Hiển thị danh sách người dùng đã subscribe
        [HttpGet]
        public async Task<IActionResult> Subscribers()
        {
            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}/api/notifications/subscribers";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to load subscribers.";
                return View(new List<UserNotificationDto>());
            }

            var result = await response.Content.ReadFromJsonAsync<List<UserNotificationDto>>();
            return View(result ?? new List<UserNotificationDto>());
        }

        // ✅ 2. Gửi đồng loạt thông báo
        [HttpPost]
        public async Task<IActionResult> SendBulk(BulkNotificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid input.");

            var success = await _notificationService.SendBulkAsync(dto);

            return Json(new
            {
                success,
                message = success ? "Notification sent successfully." : "Failed to send notification."
            });
        }

        // ✅ 3. Lịch sử tất cả các thông báo (không lọc campaign)
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}/api/notifications";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load notifications.";
                return View(new List<UserNotificationDto>());
            }

            var result = await response.Content.ReadFromJsonAsync<List<UserNotificationDto>>();
            return View(result ?? new List<UserNotificationDto>());
        }

        // ✅ 4. Lọc theo campaignId
        [HttpGet]
        public async Task<IActionResult> ByCampaign(int campaignId)
        {
            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}/api/notifications/by-campaign/{campaignId}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load campaign notifications.";
                return View(new List<UserNotificationDto>());
            }

            var result = await response.Content.ReadFromJsonAsync<List<UserNotificationDto>>();
            ViewBag.CampaignId = campaignId;
            return View(result ?? new List<UserNotificationDto>());
        }
    }
}
