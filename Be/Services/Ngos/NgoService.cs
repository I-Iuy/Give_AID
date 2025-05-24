using Be.Dtos.Ngos;
using Be.Models;
using Be.Repositories.Ngos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be.Services.Ngos
{
    public class NgoService : INgoService
    {
        private readonly INgoRepository _repo;

        public NgoService(INgoRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<NgoDto>> GetAllAsync()
        {
            var ngos = await _repo.GetAllAsync();
            return ngos.Select(n => new NgoDto
            {
                NgoId = n.NgoId,
                Name = n.Name,
                LogoUrl = n.LogoUrl,
                WebsiteUrl = n.WebsiteUrl,
                AccountId = n.AccountId
            });
        }

        public async Task<NgoDto> GetByIdAsync(int id)
        {
            var n = await _repo.GetByIdAsync(id);
            return new NgoDto
            {
                NgoId = n.NgoId,
                Name = n.Name,
                LogoUrl = n.LogoUrl,
                WebsiteUrl = n.WebsiteUrl,
                AccountId = n.AccountId
            };
        }
        private bool IsValidImage(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext == ".png" || ext == ".svg";
        }
        private async Task<bool> IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return false;

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private bool IsNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be empty or whitespace.");
            return true;
        }
        private bool IsLogoUrlValid(string logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
            {
                throw new ArgumentException("LogoUrl must not be empty or whitespace.");
            }
            if (!IsValidImage(logoUrl))
            {
                throw new ArgumentException("Logo must be .png or .svg");
            }
            return true;
        }

        private async Task<bool> IsWebUrlValid(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("WebsiteUrl must not be empty or whitespace.");
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("WebsiteUrl must be a valid URL format.");
            }

            if (!await IsValidUrl(url))
                throw new ArgumentException("WebsiteUrl is unreachable or does not exist.");

            return true;
        }


        private async Task<bool> IsNameUnique(string name, int? excludeNgoId = null)
        {
            var all = await _repo.GetAllAsync();
            bool exists = all.Any(n =>
                (excludeNgoId == null || n.NgoId != excludeNgoId) &&
                string.Equals(n.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
                throw new ArgumentException("A NGO with the same name already exists.");
            return true;
        }
        private async Task<bool> IsWebUrlUnique(string url, int? excludeNgoId = null)
        {
            var all = await _repo.GetAllAsync();
            bool exists = all.Any(n =>
                (excludeNgoId == null || n.NgoId != excludeNgoId) &&
                string.Equals(n.WebsiteUrl?.Trim(), url?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
                throw new ArgumentException("A NGO with the same WebsiteUrl already exists.");

            return true;
        }

        public async Task AddAsync(CreateNgoDto dto)
        {
            IsNameValid(dto.Name);
            IsLogoUrlValid(dto.LogoUrl);
            await IsWebUrlValid(dto.WebsiteUrl);
            await IsWebUrlUnique(dto.WebsiteUrl);
            await IsNameUnique(dto.Name);

            var ngo = new Ngo
            {
                Name = dto.Name.Trim(),
                LogoUrl = dto.LogoUrl,
                WebsiteUrl = dto.WebsiteUrl,
                AccountId = dto.AccountId
            };
            await _repo.AddAsync(ngo);
        }

        public async Task EditAsync(UpdateNgoDto dto)
        {
            IsNameValid(dto.Name);
            IsLogoUrlValid(dto.LogoUrl);
            await IsWebUrlValid(dto.WebsiteUrl);
            await IsWebUrlUnique(dto.WebsiteUrl, dto.NgoId);
            await IsNameUnique(dto.Name, dto.NgoId);

            var existing = await _repo.GetByIdAsync(dto.NgoId);
            if (existing == null)
                throw new ArgumentException("Ngo not found.");

            existing.Name = dto.Name.Trim();
            existing.LogoUrl = dto.LogoUrl;
            existing.WebsiteUrl = dto.WebsiteUrl;
            existing.AccountId = dto.AccountId;

            await _repo.EditAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
