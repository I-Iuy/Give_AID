using Fe.Dtos.Ngos;
using Newtonsoft.Json;
using System.Text;

namespace Fe.Services.Ngos
{
    public class NgoApiService : INgoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _logoPngsFolder = "wwwroot/images/logos/pngs";
        private readonly string _logoSvgsFolder = "wwwroot/images/logos/svgs";
        public NgoApiService(HttpClient httpClient, IConfiguration configuration)
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
        // Delete the logo file based on the URL provided
        private void DeleteFileLogo(string logoUrl)
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
        }
        // Get all NGOs from the API
        public async Task<IEnumerable<NgoDto>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/ngo");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<NgoDto>>(json);
        }
        // Get an NGO by ID from the API
        public async Task<NgoDto> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/ngo/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<NgoDto>(json);
        }
        // Add a new NGO from the API
        public async Task AddAsync(CreateNgoDto dto, IFormFile logo)
        {
         
            dto.LogoUrl = await SaveLogoFileAsync(logo);

            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/ngo", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }
        }
        // Check if an NGO is in use by ID from the API
        public async Task<bool> CheckInUseAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/ngo/{id}/is-used");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(errorMessage);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(content);

            return result.isUsed == true;
        }
        // Edit an existing NGO from the API
        public async Task EditAsync(UpdateNgoDto dto, IFormFile logo)
        {    
            var oldNgo = await GetByIdAsync(dto.NgoId);

            string oldLogoUrl = oldNgo.LogoUrl;

            if (logo != null && logo.Length > 0)
            {
                dto.LogoUrl = await SaveLogoFileAsync(logo);
     
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
        // Delete an NGO by ID from the API
        public async Task DeleteAsync(int id)
        {
            
            var ngo = await GetByIdAsync(id); 

            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/ngo/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    throw new InvalidOperationException(errorMessage);
                throw new HttpRequestException(errorMessage);
            }

            DeleteFileLogo(ngo.LogoUrl);
        }


    }
}
