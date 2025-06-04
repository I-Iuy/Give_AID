using Be.DTOs.Donations;
using Be.Models;
using Be.Repositories.Donations;
using Microsoft.EntityFrameworkCore;

namespace Be.Services.Donations
{
    public class DonationService : IDonationService
    {
        private readonly IDonationRepository _repo;
        private readonly DatabaseContext _context;

        public DonationService(IDonationRepository repo, DatabaseContext context)
        {
            _repo = repo;
            _context = context;
        }

        // Get all donations
        public async Task<IEnumerable<DonationDto>> GetAllAsync()
        {
            var donations = await _repo.GetAllAsync();
            var purposes = await _context.Purposes.ToListAsync();
            var campaigns = await _context.Campaigns.ToListAsync();

            return donations.Select(d => new DonationDto
            {
                DonationId = d.DonationId,
                Amount = d.Amount,
                DonatedAt = d.DonatedAt,
                Method = d.Method,
                Status = d.Status,
                AccountId = d.AccountId,
                FullName = d.FullName,
                Email = d.Email,
                Address = d.Address,
                PurposeId = d.PurposeId,
                CampaignId = d.CampaignId,
                PurposeTitle = purposes.FirstOrDefault(p => p.PurposeId == d.PurposeId)?.Title,
                CampaignTitle = d.CampaignId.HasValue
                    ? campaigns.FirstOrDefault(c => c.CampaignId == d.CampaignId)?.Title
                    : null
            });
        }

        // Get donation by ID
        public async Task<DonationDto> GetByIdAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null)
                throw new ArgumentException("Donation not found.");

            var purpose = await _context.Purposes
                .FirstOrDefaultAsync(p => p.PurposeId == d.PurposeId);

            string? campaignTitle = null;
            if (d.CampaignId.HasValue)
            {
                campaignTitle = await _context.Campaigns
                    .Where(c => c.CampaignId == d.CampaignId)
                    .Select(c => c.Title)
                    .FirstOrDefaultAsync();
            }

            return new DonationDto
            {
                DonationId = d.DonationId,
                Amount = d.Amount,
                DonatedAt = d.DonatedAt,
                Method = d.Method,
                Status = d.Status,
                AccountId = d.AccountId,
                FullName = d.FullName,
                Email = d.Email,
                Address = d.Address,
                PurposeId = d.PurposeId,
                CampaignId = d.CampaignId,
                PurposeTitle = purpose?.Title,
                CampaignTitle = campaignTitle
            };
        }
        // Check if Full Name, Email, and Address are valid
        private bool IsFullNameValid(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Full Name must not be empty or whitespace.");
            return true;
        }
        private bool IsEmailValid(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must not be empty or whitespace.");
            return true;
        }
        private bool IsAddressValid(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address must not be empty or whitespace.");
            return true;
        }
        // Check if Full Name, Email, and Address lengths are valid
        private bool IsFullNameLengthValid(string name)
        {
            if (name.Length > 100)
                throw new ArgumentException("Full Name must not exceed 100 characters.");
            return true;
        }
        private bool IsEmailLengthValid(string email)
        {
            if (email.Length > 200)
                throw new ArgumentException("Email must not exceed 200 characters.");
            return true;
        }
        private bool IsAddressLengthValid(string address)
        {
            if (address.Length > 250)
                throw new ArgumentException("Address must not exceed 250 characters.");
            return true;
        }
        // Check if Full Name, Email, and Address are valid for anonymous and logged-in donors
        private void ValidateAnonymousDonor(CreateDonationDto dto)
        {
            IsFullNameValid(dto.FullName);
            IsEmailValid(dto.Email);
            IsAddressValid(dto.Address);
            IsFullNameLengthValid(dto.FullName!);
            IsEmailLengthValid(dto.Email!);
            IsAddressLengthValid(dto.Address!);
        }
        private void ValidateLoggedInDonor(CreateDonationDto dto)
        {
            IsFullNameValid(dto.FullName);
            IsEmailValid(dto.Email);
            IsAddressValid(dto.Address);
            IsFullNameLengthValid(dto.FullName!);
            IsEmailLengthValid(dto.Email!);
            IsAddressLengthValid(dto.Address!);
        }
        // Check if Status is valid
        private bool IsStatusValid(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status must not be empty.");

            var validStatuses = new[] { "Success", "Failed" };
            if (!validStatuses.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Status must be either 'Success' or 'Failed'.");

            return true;
        }
        // Check if Amount is valid
        private bool IsAmountValid(float amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0.");
            return true;
        }
        // Add a new donation
        public async Task AddAsync(CreateDonationDto dto)
        {
            IsAmountValid(dto.Amount);
            IsStatusValid(dto.Status);
            if (dto.AccountId != null)
            {
                ValidateLoggedInDonor(dto);
            }
            else
            {
                ValidateAnonymousDonor(dto);
            }

            var purposeExists = await _context.Purposes.AnyAsync(p => p.PurposeId == dto.PurposeId);
            if (!purposeExists)
                throw new ArgumentException("Invalid purpose ID.");

            if (dto.CampaignId.HasValue)
            {
                var campaignExists = await _context.Campaigns.AnyAsync(c => c.CampaignId == dto.CampaignId);
                if (!campaignExists)
                    throw new ArgumentException("Invalid campaign ID.");
            }

            var donation = new Donation
            {
                Amount = dto.Amount,
                Method = dto.Method,
                Status = dto.Status,
                DonatedAt = DateTime.UtcNow,
                AccountId = dto.AccountId,
                FullName = dto.FullName,
                Email = dto.Email,
                Address = dto.Address,
                PurposeId = dto.PurposeId,
                CampaignId = dto.CampaignId
            };

            await _repo.AddAsync(donation);
        }
    }
}
