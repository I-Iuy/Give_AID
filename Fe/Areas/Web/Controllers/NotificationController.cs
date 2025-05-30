using Fe.DTOs.Notification;
using Fe.Services.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fe.Areas.Web.Controllers
{
    [Area("Web")]
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe()
        {
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(accountIdStr, out int accountId))
            {
                TempData["ErrorMessage"] = "Unable to identify user.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }

            var dto = new CreateNotificationDto
            {
                AccountId = accountId,
                Title = "Subscription Confirmed",
                Message = "You are now subscribed to receive notifications from our platform."
            };

            var success = await _notificationService.SendToUserAsync(dto);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "You have successfully subscribed to notifications."
                : "Failed to subscribe. Please try again later.";

            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }
    }
}
