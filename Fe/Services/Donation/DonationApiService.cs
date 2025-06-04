using Fe.DTOs.Donations;
using Newtonsoft.Json;
using System.Text;

namespace Fe.Services.Donations
{
    public class DonationApiService : IDonationApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DonationApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }

        // Get all donations from the API
        public async Task<List<DonationDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/donation");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DonationDto>>(json) ?? new List<DonationDto>();
        }

        // Get donation by ID from the API
        public async Task<DonationDto?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/donation/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DonationDto>(json);
        }

        // Add a new donation from the API
        public async Task Add(CreateDonationDto dto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/donation", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }
    }
}
