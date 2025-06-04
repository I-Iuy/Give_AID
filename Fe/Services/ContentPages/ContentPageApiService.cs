namespace Fe.Services.ContentPages
{
    using Fe.DTOs.ContentPages;
    using Newtonsoft.Json;
    using System.Net.Http.Headers;
    using System.Text;

    // Service to interact with the ContentPage API
    public class ContentPageApiService : IContentPageApiService
    {
        private readonly HttpClient _httpClient; // Used to send HTTP requests
        private readonly string _baseUrl;        // Base URL of the API server

        // Constructor initializes the HTTP client and loads the base URL from configuration
        public ContentPageApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiSettings:BaseUrl"];
        }

        // GET: Fetches all content pages from the API
        public async Task<List<ContentPageDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/contentpage");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(content);

            return JsonConvert.DeserializeObject<List<ContentPageDto>>(content);
        }

        // GET: Fetches a single content page by its ID
        public async Task<ContentPageDto?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/contentpage/{id}");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(content);

            return JsonConvert.DeserializeObject<ContentPageDto>(content);
        }

        // GET: Fetches a single content page by its slug (for routing by name)
        public async Task<ContentPageDto?> GetBySlugAsync(string slug)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/contentpage/slug/{slug}");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(content);

            return JsonConvert.DeserializeObject<ContentPageDto>(content);
        }

        // POST: Sends a request to create a new content page
        public async Task AddAsync(ContentPageDto dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/contentpage", content);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        // PUT: Sends a request to update an existing content page
        public async Task UpdateAsync(ContentPageDto dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/contentpage/{dto.Id}", content);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        // DELETE: Deletes a content page by its ID
        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/contentpage/{id}");
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }
    }
}
