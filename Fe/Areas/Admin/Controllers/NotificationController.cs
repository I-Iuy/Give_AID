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

        // ✅ 1. Display subscriber list
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

        // ✅ 2. Send bulk notification
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

        // ✅ 3. Notification history
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}/api/notifications";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to load notification history.";
                return View(new List<UserNotificationDto>());
            }

            var result = await response.Content.ReadFromJsonAsync<List<UserNotificationDto>>();
            return View(result ?? new List<UserNotificationDto>());
        }

        // ✅ 4. Notifications by campaign
        [HttpGet]
        public async Task<IActionResult> ByCampaign(int campaignId)
        {
            var apiUrl = $"{_config["ApiSettings:BaseUrl"]}/api/notifications/by-campaign/{campaignId}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to load campaign notifications.";
                return View(new List<UserNotificationDto>());
            }

            var result = await response.Content.ReadFromJsonAsync<List<UserNotificationDto>>();
            ViewBag.CampaignId = campaignId;
            return View(result ?? new List<UserNotificationDto>());
        }

        // ✅ 5. Send the latest campaign as notification
        [HttpPost]
        public async Task<IActionResult> SendLatestCampaign()
        {
            var latest = await _notificationService.GetLatestCampaignFromApi();
            if (latest == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No campaign found."
                });
            }

            var dto = new BulkNotificationDto
            {
                Title = latest.Title,
                Message = latest.Content,
                CampaignId = latest.CampaignId
            };

            var success = await _notificationService.SendBulkAsync(dto);

            return Json(new
            {
                success,
                message = success ? "Latest campaign notification sent successfully." : "Failed to send latest campaign notification."
            });
        }
    }
}
