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
        private readonly string _cmpImgFolder = "wwwroot/images/cmpcontents";
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
        private List<(byte[] Data, string Extension, string FileName)> ExtractImagesFromContent(ref string htmlContent)
        {
            var result = new List<(byte[], string, string)>();

            var imgRegex = new Regex("<img[^>]+src=[\"']data:image/(?<ext>[^;]+);base64,(?<data>[^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase);
            htmlContent = imgRegex.Replace(htmlContent, match =>
            {
                var ext = match.Groups["ext"].Value.ToLower();
                var base64Data = match.Groups["data"].Value;
                byte[] bytes = Convert.FromBase64String(base64Data);

                string newFileName = GenerateFileName($"image.{ext}");
                result.Add((bytes, ext, newFileName));

                string relativePath = $"/images/cmpcontents/{newFileName}";
                return $"<img src=\"{relativePath}\" />";
            });

            return result;
        }
        // Lưu hình ảnh vào thư mục wwwroot/images/cmpcontents/pngs hoặc svgs
        public async Task<string> SaveImgContent(string contentHtml)
        {
            if (string.IsNullOrWhiteSpace(contentHtml)) return null;

            var imageList = ExtractImagesFromContent(ref contentHtml);

            Directory.CreateDirectory(_cmpImgFolder);

            foreach (var (data, ext, fileName) in imageList)
            {
                string fullPath = Path.Combine(_cmpImgFolder, fileName);
                await File.WriteAllBytesAsync(fullPath, data);
            }

            return contentHtml;
        }
        // Xóa hình ảnh từ thư mục wwwwroot/images/cmpcontents/pngs hoặc svgs 
        public void DeleteImgContent(string contentHtml)
        {
            if (string.IsNullOrWhiteSpace(contentHtml))
                return;

            var imgSrcRegex = new Regex("<img[^>]+src=[\"'](?<src>/images/cmpcontents/(?<filename>[^\"']+))[\"'][^>]*>", RegexOptions.IgnoreCase);

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
                        throw new HttpRequestException(ex.Message);
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
        public async Task EditAsync(UpdateCampaignDto dto)
        {
            // Lấy nội dung cũ từ API
            var existingCampaign = await GetByIdAsync(dto.CampaignId);
            var oldContent = existingCampaign.Content;

            if (!string.Equals(oldContent?.Trim(), dto.Content?.Trim(), StringComparison.Ordinal))
            {
                // So sánh danh sách ảnh cũ và mới bằng Regex
                var imgRegex = new Regex("<img[^>]+src=[\"'](?<src>/images/cmpcontents/[^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase);

                var oldImgs = imgRegex.Matches(oldContent ?? "")
                                       .Select(m => m.Groups["src"].Value)
                                       .ToList();

                var newImgs = imgRegex.Matches(dto.Content ?? "")
                                       .Select(m => m.Groups["src"].Value)
                                       .ToList();

                var removedImgs = oldImgs.Except(newImgs, StringComparer.OrdinalIgnoreCase).ToList();

                // Tạo nội dung giả chỉ chứa ảnh bị xoá
                string fakeContent = string.Join("", removedImgs.Select(src => $"<img src=\"{src}\" />"));

                // Gọi lại phương thức xoá sẵn có
                DeleteImgContent(fakeContent);

                // Cuối cùng, xử lý ảnh base64 → ảnh mới
                dto.Content = await SaveImgContent(dto.Content);
            }

            // Gửi yêu cầu cập nhật
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/campaign", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error updating campaign: {error}");
            }
        }

        // DELETE: Xoá Campaign theo ID
        public async Task DeleteAsync(int id)
        {
            // Lấy thông tin campaign để xoá ảnh
            var campaign = await GetByIdAsync(id);

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/campaign/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }

            // Nếu xoá thành công → xoá ảnh trong nội dung
            DeleteImgContent(campaign.Content);
        }


    }
}
