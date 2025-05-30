using Fe.DTOs.Share;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;

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
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/share", dto);
            return response.IsSuccessStatusCode;
        }
    }
}
