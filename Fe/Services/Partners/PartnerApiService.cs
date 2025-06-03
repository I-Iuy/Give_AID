using Fe.DTOs.Partners;
using Newtonsoft.Json;
using System.Text;

namespace Fe.Services.Partners
{
    public class PartnerApiService : IPartnerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _logoPngsFolder = "wwwroot/images/logos/pngs";
        private readonly string _logoSvgsFolder = "wwwroot/images/logos/svgs";
        private readonly string _fileDocxFolder = "wwwroot/files/docxs";
        private readonly string _filePdfFolder = "wwwroot/files/pdfs";

        public PartnerApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"];
        }
        // Create FileName
        private string GenerateFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);

            var now = DateTime.Now;
            var timestamp = $"{now:yyMMdd_HHmmss}"; 

            return $"{fileNameWithoutExt}_{timestamp}{extension}";
        }
        // Save the logo file to the appropriate folder based on its extension
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

            var relativePath = fullPath.Replace("wwwroot", "").Replace("\\", "/");
            return relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        }
        // Save the contract file to the appropriate folder based on its extension
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

            var relativePath = fullPath.Replace("wwwroot", "").Replace("\\", "/");
            return relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        }
        // Get the logo file stream based on the URL provided
        public FileStream GetLogoFileStream(string logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
                throw new ArgumentException("Logo URL is empty.");

            var relativePath = logoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
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
        // Get the contract file stream based on the URL provided
        public FileStream GetContractFileStream(string contractFileUrl)
        {
            if (string.IsNullOrWhiteSpace(contractFileUrl))
                throw new ArgumentException("Contract file URL is empty.");

            var relativePath = contractFileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var extension = Path.GetExtension(relativePath).ToLower();

            string fullPath = extension switch
            {
                ".pdf" => Path.Combine(_filePdfFolder, Path.GetFileName(relativePath)),
                ".docx" => Path.Combine(_fileDocxFolder, Path.GetFileName(relativePath)),
                _ => throw new InvalidOperationException("Unsupported contract file type.")
            };

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Contract file not found.", fullPath);

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }
        // Delete files from the server based on the provided URLs
        private void DeleteFileAll(string logoUrl, string contractFileUrl)
        {
            void DeleteFile(string url)
            {
                if (string.IsNullOrWhiteSpace(url)) return;

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
            DeleteFile(contractFileUrl);
        }

        // Get all Partners from the API
        public async Task<IEnumerable<PartnerDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/partner");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<PartnerDto>>(json);
        }
        // Get Partner by ID from the API
        public async Task<PartnerDto> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/partner/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PartnerDto>(json);
        }

        // Add a new Partner from the API
        public async Task AddAsync(CreatePartnerDto dto, IFormFile logo, IFormFile contract)
        {
            dto.LogoUrl = await SaveLogoFileAsync(logo);
            dto.ContractFile = await SaveContractFileAsync(contract);

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/partner", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }
        // Check if a Partner is in use by ID from the API
        public async Task<bool> CheckInUseAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/partner/{id}/is-used");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(content);

            return result.isUsed == true;
        }
        // Edit an existing Partner from the API
        public async Task EditAsync(UpdatePartnerDto dto, IFormFile logo, IFormFile contract)
        {      
            var oldPartner = await GetByIdAsync(dto.PartnerId);

            string oldLogoUrl = oldPartner.LogoUrl;
            string oldContractFile = oldPartner.ContractFile;

            if (logo != null && logo.Length > 0)
            {
                dto.LogoUrl = await SaveLogoFileAsync(logo);

                DeleteFileAll(oldLogoUrl, null);
            }

            if (contract != null && contract.Length > 0)
            {
                dto.ContractFile = await SaveContractFileAsync(contract);
                DeleteFileAll(null, oldContractFile);
            }

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/partner", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }
        // Delete a Partner by ID from the API
        public async Task DeleteAsync(int id)
        {
            var partner = await GetByIdAsync(id);
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/partner/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    throw new InvalidOperationException(errorMessage); 
                throw new HttpRequestException(errorMessage);
            }

            DeleteFileAll(partner.LogoUrl, partner.ContractFile);
        }

    }
}