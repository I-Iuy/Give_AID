using Fe.Dtos.Ngos;
using Fe.DTOs.Campaigns;
using Fe.DTOs.Partners;
using Fe.DTOs.Purposes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Fe.Services.Campaigns
{
    public class CampaignApiService : ICampaignApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _cmpPngsFolder = "wwwroot/images/cmpcontents/pngs";
        private readonly string _cmpSvgsFolder = "wwwroot/images/cmpcontents/svgs";
        public CampaignApiService(HttpClient httpClient, IConfiguration configuration)
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
        // Xử lý lọc Content -> lấy hình ảnh từ nội dung HTML của Campaign
        private List<(byte[] Data, string Extension)> ExtractImagesFromContent(ref string htmlContent)
        {
            var result = new List<(byte[] Data, string Extension)>();

            var imgRegex = new Regex("<img[^>]+src=[\"']data:image/(?<ext>[^;]+);base64,(?<data>[^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase);
            htmlContent = imgRegex.Replace(htmlContent, match =>
            {
                var ext = match.Groups["ext"].Value.ToLower(); 
                var base64Data = match.Groups["data"].Value;
                byte[] bytes = Convert.FromBase64String(base64Data);

                result.Add((bytes, ext));

                string newFileName = GenerateFileName($"image.{ext}");
                string relativePath = ext switch
                {
                    "png" => $"/images/cmpcontents/pngs/{newFileName}",
                    "svg" => $"/images/cmpcontents/svgs/{newFileName}",
                    _ => throw new InvalidOperationException("Only png/svg images are supported.")
                };

                return $"<img src=\"{relativePath}\" />";
            });

            return result;
        }
        // Lưu hình ảnh vào thư mục wwwroot/images/cmpcontents/pngs hoặc svgs
        public async Task<string> SaveImgContent(string contentHtml)
        {
            if (string.IsNullOrWhiteSpace(contentHtml)) return null;

            // Extract và lấy danh sách ảnh từ Content
            var imageList = ExtractImagesFromContent(ref contentHtml);

            foreach (var (data, ext) in imageList)
            {
                string fileName = GenerateFileName($"image.{ext}");
                string folder = ext switch
                {
                    "png" => _cmpPngsFolder,
                    "svg" => _cmpSvgsFolder,
                    _ => throw new InvalidOperationException("Unsupported image format.")
                };

                Directory.CreateDirectory(folder);
                var fullPath = Path.Combine(folder, fileName);

                await File.WriteAllBytesAsync(fullPath, data);
            }

            return contentHtml;
        }
        // Lấy hình ảnh từ thư mục wwwroot/images/cmpcontents/pngs hoặc svgs
        public FileStream GetImgContent(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL is empty.");

            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var extension = Path.GetExtension(relativePath).ToLower();

            string fullPath = extension switch
            {
                ".png" => Path.Combine(_cmpPngsFolder, Path.GetFileName(relativePath)),
                ".svg" => Path.Combine(_cmpSvgsFolder, Path.GetFileName(relativePath)),
                _ => throw new InvalidOperationException("Unsupported image file type.")
            };

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Image not found.", fullPath);

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }
        // Xóa hình ảnh từ thư mục wwwwroot/images/cmpcontents/pngs hoặc svgs 
        public void DeleteImgContent(string contentHtml)
        {
            if (string.IsNullOrWhiteSpace(contentHtml))
                return;

            var imgSrcRegex = new Regex("<img[^>]+src=[\"'](?<src>/images/cmpcontents/(?<type>pngs|svgs)/(?<filename>[^\"']+))[\"'][^>]*>", RegexOptions.IgnoreCase);

            foreach (Match match in imgSrcRegex.Matches(contentHtml))
            {
                var url = match.Groups["src"].Value; 

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
                        throw new IOException($"Lỗi khi xoá ảnh: {fullPath}. Chi tiết: {ex.Message}");
                    }
                }
            }
        }

        // GET: lấy danh sách tất cả các Campaigns
        public async Task<IEnumerable<CampaignDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/campaign");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<CampaignDto>>(json);
        }
   

        // GET: lấy Campaign theo ID
        public async Task<CampaignDto> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/campaign/{id}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CampaignDto>(json);
        }
        // POST: tạo mới Campaign
        public async Task AddAsync(CreateCampaignDto dto)
        {
            dto.Content = await SaveImgContent(dto.Content);
            dto.EventDate = DateTime.Now;
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/campaign", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error creating campaign: {error}");
            }
        }
    }
}
