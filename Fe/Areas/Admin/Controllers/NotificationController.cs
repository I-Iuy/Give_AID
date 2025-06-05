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
        public async Task<IActionResult> Subscribers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // Subscribers list will now use the general paginated notifications endpoint for simplicity
            // In a real application, you might need a specific endpoint for only 'subscribed' users if that's different
            var (notifications, totalCount) = await _notificationService.GetPaginatedNotificationsAsync(pageNumber, pageSize);

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(notifications);
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
        public async Task<IActionResult> History([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var (notifications, totalCount) = await _notificationService.GetPaginatedNotificationsAsync(pageNumber, pageSize);

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(notifications);
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

        // ✅ 5. Get latest campaign info
        [HttpGet]
        public async Task<IActionResult> GetLatestCampaign()
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

            return Json(new
            {
                success = true,
                campaign = new
                {
                    title = latest.Title,
                    eventDate = latest.EventDate,
                    campaignId = latest.CampaignId
                }
            });
        }

        // ✅ 6. Send the latest campaign as notification
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
                Title = $"New Campaign: {latest.Title}",
                Message = $"A new campaign has been created: {latest.Title}. Click here to view details.",
                CampaignId = latest.CampaignId
            };

            var success = await _notificationService.SendBulkAsync(dto);

            return Json(new
            {
                success,
                message = success 
                    ? $"Campaign notification sent successfully to all subscribers." 
                    : "Failed to send campaign notification."
            });
        }
    }
}
