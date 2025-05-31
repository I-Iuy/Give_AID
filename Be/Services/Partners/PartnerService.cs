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
        private bool IsValidImage(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext == ".png" || ext == ".svg";
        }

        private bool IsValidDocument(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext == ".pdf" || ext == ".docx";
        }
        private bool IsNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty or whitespace.");
            }
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
        public async Task AddAsync(CreatePartnerDto dto)
        {
            // Kiểm tra validation
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

        public async Task EditAsync(UpdatePartnerDto dto)
        {
            // Kiểm tra validation
            IsNameValid(dto.Name);
            IsLogoUrlValid(dto.LogoUrl);
            IsContractFileValid(dto.ContractFile);
            await IsNameUnique(dto.Name, dto.PartnerId);

            // Tìm entity đang được tracking
            var existing = await _repo.GetByIdAsync(dto.PartnerId);
            if (existing == null)
                throw new ArgumentException("Partner not found.");

            // Cập nhật dữ liệu trực tiếp trên entity đã được track
            existing.Name = dto.Name.Trim();
            existing.LogoUrl = dto.LogoUrl;
            existing.ContractFile = dto.ContractFile;
            existing.AccountId = dto.AccountId;

            await _repo.EditAsync(existing); 
        }


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
