using Be.DTOs.Campaigns;
using Be.Models;
using Be.Repositories.Campaigns;
using Microsoft.EntityFrameworkCore;

namespace Be.Services.Campaigns
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _repo;
        private readonly DatabaseContext _context;
        public CampaignService(ICampaignRepository repo, DatabaseContext context)
        {
            _repo = repo;
            _context = context;
        }
        public async Task<IEnumerable<CampaignDto>> GetAllAsync()
        {
            var campaigns = await _repo.GetAllAsync();
            var purposes = await _context.Purposes.ToListAsync();

            return campaigns.Select(n => new CampaignDto
            {
                CampaignId = n.CampaignId,
                Title = n.Title,
                Content = n.Content,
                VideoUrl = n.VideoUrl,
                EventDate = n.EventDate,
                AccountId = n.AccountId,
                PurposeTitle = purposes.FirstOrDefault(p => p.PurposeId == n.PurposeId)?.Title
            });
        }
        public async Task<CampaignDto> GetByIdAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null)
                throw new ArgumentException("Campaign not found.");

            var purpose = await _context.Purposes
                .FirstOrDefaultAsync(p => p.PurposeId == c.PurposeId);

            var partnerIds = await _context.CampaignPartners
                .Where(x => x.CampaignId == id)
                .Select(x => x.PartnerId)
                .ToListAsync();

            var ngoIds = await _context.CampaignNgos
                .Where(x => x.CampaignId == id)
                .Select(x => x.NgoId)
                .ToListAsync();

            var partnerNames = await _context.Partners
                .Where(p => partnerIds.Contains(p.PartnerId))
                .Select(p => p.Name)
                .ToListAsync();

            var ngoNames = await _context.Ngos
                .Where(n => ngoIds.Contains(n.NgoId))
                .Select(n => n.Name)
                .ToListAsync();

            return new CampaignDto
            {
                CampaignId = c.CampaignId,
                Title = c.Title,
                Content = c.Content,
                VideoUrl = c.VideoUrl,
                EventDate = c.EventDate,
                PurposeTitle = purpose?.Title,
                NgoNames = ngoNames,
                PartnerNames = partnerNames,
                AccountId = c.AccountId
            };
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
        private bool IsTitleValid(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title must not be empty or whitespace.");
            return true;
        }
        private bool IsContentValid(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content must not be empty or whitespace.");
            return true;
        }
        private async Task<bool> IsVideoUrlValid(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("VideoUrl must not be empty or whitespace.");
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("VideoUrl must be a valid URL format.");
            }

            if (!await IsValidUrl(url))
                throw new ArgumentException("VideoUrl is unreachable or does not exist.");

            return true;
        }
        private async Task<bool> IsTitleUnique(string title, int? excludeCampaignId = null)
        {
            var all = await _repo.GetAllAsync();
            bool exists = all.Any(n =>
                (excludeCampaignId == null || n.CampaignId != excludeCampaignId) &&
                string.Equals(n.Title.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
                throw new ArgumentException("A campaign with the same title already exists.");
            return true;
        }
        private async Task<bool> IsVideoUrlUnique(string url, int? excludeCampaignId = null)
        {
            var all = await _repo.GetAllAsync();
            bool exists = all.Any(c =>
                (excludeCampaignId == null || c.CampaignId != excludeCampaignId) &&
                string.Equals(c.VideoUrl?.Trim(), url?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exists)
                throw new ArgumentException("A campaign with the same VideoUrl already exists.");

            return true;
        }
        private async Task<bool> IsPurposeExists(int purposeId)
        {
            if (purposeId <= 0)
                return false;

            return await _context.Purposes.AnyAsync(p => p.PurposeId == purposeId);
        }
        private async Task<bool> ArePartnersValid(List<int> partnerIds)
        {
            if (partnerIds == null || !partnerIds.Any()) return false;

            var existingCount = await _context.Partners
                .CountAsync(p => partnerIds.Contains(p.PartnerId));

            return existingCount == partnerIds.Count;
        }
        private async Task<bool> AreNgosValid(List<int> ngoIds)
        {
            if (ngoIds == null || !ngoIds.Any()) return false;

            var existingCount = await _context.Ngos
                .CountAsync(n => ngoIds.Contains(n.NgoId));

            return existingCount == ngoIds.Count;
        }
        public async Task AddAsync(CreateCampaignDto dto)
        {
            IsTitleValid(dto.Title);
            IsContentValid(dto.Content);
            await IsVideoUrlValid(dto.VideoUrl);
            await IsTitleUnique(dto.Title);
            await IsVideoUrlUnique(dto.VideoUrl);
            if (!await IsPurposeExists(dto.PurposeId))
                throw new ArgumentException("Purpose ID is invalid.");
            if (!await ArePartnersValid(dto.PartnerIds))
                throw new ArgumentException("One or more Partner IDs are invalid.");
            if (!await AreNgosValid(dto.NgoIds))
                throw new ArgumentException("One or more NGO IDs are invalid.");
            var campaign = new Campaign
            {
                Title = dto.Title,
                Content = dto.Content,
                VideoUrl = dto.VideoUrl,
                EventDate = dto.EventDate,
                PurposeId = dto.PurposeId,
                AccountId = dto.AccountId,
                CampaignPartners = dto.PartnerIds.Select(partnerId => new CampaignPartner
                {
                    PartnerId = partnerId
                }).ToList(),
                CampaignNgos = dto.NgoIds.Select(ngoId => new CampaignNgo
                {
                    NgoId = ngoId
                }).ToList()
            };
            await _repo.AddAsync(campaign);
        }

        public async Task EditAsync(UpdateCampaignDto dto)
        {
            IsTitleValid(dto.Title);
            IsContentValid(dto.Content);
            await IsVideoUrlValid(dto.VideoUrl);
            await IsTitleUnique(dto.Title, dto.CampaignId);
            await IsVideoUrlUnique(dto.VideoUrl, dto.CampaignId);

            if (!await IsPurposeExists(dto.PurposeId))
                throw new ArgumentException("Purpose ID is invalid.");
            if (!await ArePartnersValid(dto.PartnerIds))
                throw new ArgumentException("One or more Partner IDs are invalid.");
            if (!await AreNgosValid(dto.NgoIds))
                throw new ArgumentException("One or more NGO IDs are invalid.");

            var existing = await _context.Campaigns
                .Include(c => c.CampaignNgos)
                .Include(c => c.CampaignPartners)
                .FirstOrDefaultAsync(c => c.CampaignId == dto.CampaignId);

            if (existing == null)
                throw new ArgumentException("Campaign not found.");

            existing.Title = dto.Title.Trim();
            existing.Content = dto.Content;
            existing.VideoUrl = dto.VideoUrl.Trim();
            existing.EventDate = dto.EventDate;
            existing.PurposeId = dto.PurposeId;
            existing.AccountId = dto.AccountId;

            _context.CampaignPartners.RemoveRange(existing.CampaignPartners);
            _context.CampaignNgos.RemoveRange(existing.CampaignNgos);

            existing.CampaignPartners = dto.PartnerIds.Select(id => new CampaignPartner { PartnerId = id }).ToList();
            existing.CampaignNgos = dto.NgoIds.Select(id => new CampaignNgo { NgoId = id }).ToList();

            await _repo.EditAsync(existing);
        }
        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}