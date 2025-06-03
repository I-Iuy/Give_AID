using Fe.Dtos.Ngos;
using Fe.DTOs.Partners;
using Fe.DTOs.Purposes;
using Newtonsoft.Json;

namespace Fe.Services.Getdata
{
    public class GetdataApiService : IGetdataApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        public GetdataApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }
        // Get all purposes, partners, and NGOs from the API
        public async Task<List<PurposeDto>> GetAllPurposesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/purpose");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<PurposeDto>>(json);

        }
        public async Task<List<PartnerDto>> GetAllPartnersAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/partner");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<PartnerDto>>(json);
        }
        public async Task<List<NgoDto>> GetAllNgosAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/ngo");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<NgoDto>>(json);
        }

    }
}
