using Be.DTOs.Partners;
using Be.Models;
using Be.Repositories.Partners;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Be.Services.Partners
{
    public class PartnerService : IPartnerService
    {
        private readonly IPartnerRepository _repo;
        private readonly DatabaseContext _context;
        public PartnerService(IPartnerRepository repo, DatabaseContext context)
        {
            _repo = repo;
            _context = context;
        }
        // Get all partners
        public async Task<IEnumerable<PartnerDto>> GetAllAsync()
        {
            var partners = await _repo.GetAllAsync();
            return partners.Select(p => new PartnerDto
            {
                PartnerId = p.PartnerId,
                Name = p.Name,
                LogoUrl = p.LogoUrl,
                ContractFile = p.ContractFile,
                AccountId = p.AccountId
            });
        }
        // Get partner by ID
        public async Task<PartnerDto> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return new PartnerDto
            {
                PartnerId = p.PartnerId,
                Name = p.Name,
                LogoUrl = p.LogoUrl,
                ContractFile = p.ContractFile,
                AccountId = p.AccountId
            };
        }
        // Check if Img is valid
        private bool IsValidImage(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext == ".png" || ext == ".svg";
        }
        // Check if Document is valid
        private bool IsValidDocument(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext == ".pdf" || ext == ".docx";
        }
        // Check if Name is valid 
        private bool IsNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty or whitespace.");
            }
            return true;
        }
        // Check if Logo URL is valid
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
        // Check if Contract file is valid
        private bool IsContractFileValid(string contractFile)
        {
            if (string.IsNullOrWhiteSpace(contractFile))
            {
                throw new ArgumentException("ContractFile must not be empty or whitespace.");
            }
            if (!IsValidDocument(contractFile))
            {
                throw new ArgumentException("Contract file must be .pdf or .docx");
            }
            return true;
        }
        // Check if Name is unique
        private async Task<bool> IsNameUnique(string name, int? excludePartnerId = null)
        {
            var allPartners = await _repo.GetAllAsync();
            bool isDuplicate = allPartners.Any(p =>
                (excludePartnerId == null || p.PartnerId != excludePartnerId) &&
                string.Equals(p.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                throw new ArgumentException("A partner with the same name already exists.");
            }
            return true;
        }
        // Add a new partner
        public async Task AddAsync(CreatePartnerDto dto)
        {
            IsNameValid(dto.Name);
            IsLogoUrlValid(dto.LogoUrl);
            IsContractFileValid(dto.ContractFile);
            await IsNameUnique(dto.Name);

            var partner = new Partner
            {
                Name = dto.Name.Trim(),
                LogoUrl = dto.LogoUrl,
                ContractFile = dto.ContractFile,
                AccountId = dto.AccountId
            };
            await _repo.AddAsync(partner);
        }
        // Edit an existing partner
        public async Task EditAsync(UpdatePartnerDto dto)
        {
            IsNameValid(dto.Name);
            IsLogoUrlValid(dto.LogoUrl);
            IsContractFileValid(dto.ContractFile);
            await IsNameUnique(dto.Name, dto.PartnerId);

            var existing = await _repo.GetByIdAsync(dto.PartnerId);
            if (existing == null)
                throw new ArgumentException("Partner not found.");

            existing.Name = dto.Name.Trim();
            existing.LogoUrl = dto.LogoUrl;
            existing.ContractFile = dto.ContractFile;
            existing.AccountId = dto.AccountId;

            await _repo.EditAsync(existing); 
        }
        // Delete a partner by ID (with check if it is used in campaigns)
        public async Task DeleteAsync(int id)
        {
            bool isUsed = await _context.CampaignPartners.AnyAsync(cp => cp.PartnerId == id);

            if (isUsed)
            {
                throw new InvalidOperationException("Partner is in use. Delete related campaigns first");
            }

            await _repo.DeleteAsync(id);
        }
    }
}
