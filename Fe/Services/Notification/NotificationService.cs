using Fe.DTOs.Notification;
using Fe.Services.Campaigns;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Fe.Services.Notification
{
   
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ICampaignApiService _campaignService;

        public NotificationService(HttpClient httpClient, IConfiguration config, ICampaignApiService campaignService)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiSettings:BaseUrl"] ?? throw new Exception("API base URL not found.");
            _campaignService = campaignService;
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

        public async Task<Fe.DTOs.Campaigns.CampaignDto?> GetLatestCampaignFromApi()
        {
            var allCampaigns = await _campaignService.GetAllAsync();
            return allCampaigns?.OrderByDescending(c => c.EventDate).FirstOrDefault();
        }

        public async Task<List<UserNotificationDto>> GetHistoryAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<UserNotificationDto>>($"{_baseUrl}/api/notification");
            return result ?? new List<UserNotificationDto>();
        }

        public async Task<List<UserNotificationDto>> GetByCampaignAsync(int campaignId)
        {
            var result = await _httpClient.GetFromJsonAsync<List<UserNotificationDto>>($"{_baseUrl}/api/notification/by-campaign/{campaignId}");
            return result ?? new List<UserNotificationDto>();
        }
    }
}
