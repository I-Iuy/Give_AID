using Fe.DTOs.Purposes;
using Newtonsoft.Json;
using System.Text;

namespace Fe.Services.Purposes
{
    public class PurposeApiService : IPurposeApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PurposeApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }

        // Get all Purposes from the API
        public async Task<IEnumerable<PurposeDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/purpose");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<PurposeDto>>(json);
        }
        // Get Purpose by ID from the API
        public async Task<PurposeDto> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/purpose/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PurposeDto>(json);
        }
        // Add a new Purpose from the API
        public async Task CreateAsync(CreatePurposeDto dto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/purpose", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage); 
            }
        }
        // Check if a Purpose is in use by ID from the API
        public async Task<bool> CheckInUseAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/purpose/{id}/is-used");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<dynamic>(content);
            return result.isUsed == true; 
        }
        // Edit an existing Purpose from the API
        public async Task EditAsync(UpdatePurposeDto dto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/purpose", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    throw new InvalidOperationException(errorMessage);

                throw new HttpRequestException(errorMessage);
            }
        }
        // Delete a Purpose by ID from the API
        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/purpose/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    throw new InvalidOperationException(errorMessage);
                throw new HttpRequestException(errorMessage); 
            }
        }
    }
}
