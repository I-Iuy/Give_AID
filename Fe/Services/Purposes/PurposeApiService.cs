using Fe.DTOs.Purposes;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        // Lấy danh sách Purpose
        public async Task<IEnumerable<PurposeDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/purpose");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<PurposeDto>>(json);
        }

        // Tạo mới Purpose
        public async Task<bool> CreateAsync(CreatePurposeDto dto)
        {
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/purpose", content);

            if (response.IsSuccessStatusCode)
                return true;

            var errorMessage = await response.Content.ReadAsStringAsync(); // Lấy nội dung lỗi từ BE
            throw new HttpRequestException(errorMessage); // Ném lên FE Controller
        }


        // Xóa Purpose theo ID
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/purpose/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
