using Fe.DTOs.Partners;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Fe.Services.Partners
{
    public class PartnerApiService : IPartnerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _logoPngsFolder = "wwwroot/images/logos/pngs";
        private readonly string _logoSvgsFolder = "wwwroot/files/logos/svgs";
        private readonly string _fileDocxFolder = "wwwroot/files/docxs";
        private readonly string _filePdfFolder = "wwwroot/files/pdfs";

        public PartnerApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }

        private string GenerateFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("HH_mm_dd_MM_yy");
            return $"{fileNameWithoutExt}_{timestamp}{extension}";
        }

        private async Task<string> SaveLogoFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = GenerateFileName(file.FileName);

            string folder = ext switch
            {
                ".png" => _logoPngsFolder,
                ".svg" => _logoSvgsFolder,
                _ => throw new InvalidOperationException("Only .png or .svg logos are allowed.")
            };

            Directory.CreateDirectory(folder);
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Sử dụng Path để xử lý đường dẫn một cách nhất quán
            var relativePath = fullPath.Replace("wwwroot", "").Replace("\\", "/");
            return relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        }

        private async Task<string> SaveContractFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = GenerateFileName(file.FileName);

            string folder = ext switch
            {
                ".pdf" => _filePdfFolder,
                ".docx" => _fileDocxFolder,
                _ => throw new InvalidOperationException("Only .pdf or .docx contracts are allowed.")
            };

            Directory.CreateDirectory(folder);
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Sử dụng Path để xử lý đường dẫn một cách nhất quán
            var relativePath = fullPath.Replace("wwwroot", "").Replace("\\", "/");
            return relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        }

        public FileStream GetLogoFileStream(string logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
                throw new ArgumentException("Logo URL is empty.");

            // Bỏ dấu '/' đầu tiên nếu có
            var relativePath = logoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            // Xác định loại file: .png hoặc .svg
            var extension = Path.GetExtension(relativePath).ToLower();

            string fullPath = extension switch
            {
                ".png" => Path.Combine(_logoPngsFolder, Path.GetFileName(relativePath)),
                ".svg" => Path.Combine(_logoSvgsFolder, Path.GetFileName(relativePath)),
                _ => throw new InvalidOperationException("Unsupported logo file type.")
            };

            if (!System.IO.File.Exists(fullPath))
                throw new FileNotFoundException("Logo file not found.", fullPath);

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }

        // GET: Lấy danh sách tất cả Partner
        public async Task<IEnumerable<PartnerDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/partner");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<PartnerDto>>(json);
        }

        // GET: Lấy Partner theo ID
        public async Task<PartnerDto> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/partner/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PartnerDto>(json);
        }

        // POST: Thêm Partner mới
        public async Task AddAsync(CreatePartnerDto dto, IFormFile logo, IFormFile contract)
        {
            // Lưu file logo vào wwwroot và trả về đường dẫn
            dto.LogoUrl = await SaveLogoFileAsync(logo);

            // Lưu file hợp đồng vào wwwroot và trả về đường dẫn
            dto.ContractFile = await SaveContractFileAsync(contract);

            // Gửi DTO (chỉ chứa string path) sang BE qua API
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/partner", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }

        // PUT: Cập nhật Partner
        public async Task EditAsync(UpdatePartnerDto dto, IFormFile logo, IFormFile contract)
        {
            // Nếu có file mới thì xử lý thay thế
            if (logo != null && logo.Length > 0)
            {
                dto.LogoUrl = await SaveLogoFileAsync(logo);
            }

            if (contract != null && contract.Length > 0)
            {
                dto.ContractFile = await SaveContractFileAsync(contract);
            }

            // Gửi DTO đã cập nhật đường dẫn file sang BE
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/partner", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }

        // DELETE: Xoá Partner theo ID
        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/partner/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }
    }
}