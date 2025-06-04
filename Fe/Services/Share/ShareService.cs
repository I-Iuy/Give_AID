using Fe.DTOs.Share;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fe.Services.Share
{
    public class ShareService : IShareService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ShareService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiSettings:BaseUrl"] ?? throw new Exception("API base URL not found.");
        }

        public async Task<IEnumerable<ShareDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ShareDto>>($"{_baseUrl}/api/share")
                   ?? new List<ShareDto>();
        }

        public async Task<ShareDto?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ShareDto>($"{_baseUrl}/api/share/{id}");
        }

        public async Task<bool> CreateAsync(CreateShareDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/share/sharecampaign", dto);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<dynamic>(content);
                    return true;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to share campaign: {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sharing campaign: {ex.Message}", ex);
            }
        }

        public async Task<bool> ShareCampaignAsync(CreateShareDto dto, string baseUrl)
        {
            try
            {
                dto.BaseUrl = baseUrl;
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/share/sharecampaign", dto);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<dynamic>(content);
                    return true;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to share campaign: {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sharing campaign: {ex.Message}", ex);
            }
        }
    }
}
