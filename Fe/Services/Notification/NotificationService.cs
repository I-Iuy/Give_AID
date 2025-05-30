using Fe.DTOs.Notification;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;

namespace Fe.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public NotificationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiSettings:BaseUrl"] ?? throw new Exception("API base URL not found.");
        }

        public async Task<IEnumerable<UserNotificationDto>> GetByAccountIdAsync(int accountId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserNotificationDto>>($"{_baseUrl}/api/notification/account/{accountId}")
                   ?? new List<UserNotificationDto>();
        }

        public async Task<bool> SendToUserAsync(CreateNotificationDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/notification/user", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendBulkAsync(BulkNotificationDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/notification/bulk", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/notification/{notificationId}/read", null);
            return response.IsSuccessStatusCode;
        }
    }
}
