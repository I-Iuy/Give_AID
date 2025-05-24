using Fe.Dtos.Ngos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fe.Services.Ngos
{
    public class NgoApiService : INgoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _logoPngsFolder = "wwwroot/images/logos/pngs";
        private readonly string _logoSvgsFolder = "wwwroot/files/logos/svgs";
        public NgoApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }
        private string GenerateFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);

            var now = DateTime.Now;
            var timestamp = $"{now:yyMMdd_HHmmss}";

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
        private void DeleteFileLogo(string logoUrl)
        {
            void DeleteFile(string url)
            {
                if (string.IsNullOrWhiteSpace(url)) return;

                // Chuẩn hoá đường dẫn tuyệt đối từ wwwroot
                var relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine("wwwroot", relativePath);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        throw new HttpRequestException(ex.Message);
                    }
                }
            }

            DeleteFile(logoUrl);
        }
        // GET: Lấy danh sách tất cả Ngos
        public async Task<IEnumerable<NgoDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/ngo");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<NgoDto>>(json);
        }
        // GET: Lấy Ngo theo ID
        public async Task<NgoDto> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/ngo/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<NgoDto>(json);
        }

        // POST: Thêm Ngo mới
        public async Task AddAsync(CreateNgoDto dto, IFormFile logo)
        {
            // Lưu file logo vào wwwroot và trả về đường dẫn
            dto.LogoUrl = await SaveLogoFileAsync(logo);

            // Gửi DTO (chỉ chứa string path) sang BE qua API
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/ngo", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }
        // PUT: Cập nhật Ngo
        public async Task EditAsync(UpdateNgoDto dto, IFormFile logo)
        {
            // Lấy thông tin ngo hiện tại để biết file cũ
            var oldNgo = await GetByIdAsync(dto.NgoId);

            string oldLogoUrl = oldNgo.LogoUrl;

            // Nếu có file logo mới
            if (logo != null && logo.Length > 0)
            {
                dto.LogoUrl = await SaveLogoFileAsync(logo);
                // Xoá logo cũ sau khi lưu mới thành công
                DeleteFileLogo(oldLogoUrl); 
            }

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/ngo", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }
        // DELETE: Xoá Ngo theo ID
        public async Task DeleteAsync(int id)
        {
            // Lấy thông tin ngo trước khi xoá để lấy đường dẫn file
            var ngo = await GetByIdAsync(id); // Gọi lại API để lấy LogoUrl 

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/ngo/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }

            // Nếu xoá thành công → xoá file vật lý
            DeleteFileLogo(ngo.LogoUrl);
        }


    }
}
