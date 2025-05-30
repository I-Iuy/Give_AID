using Be.DTOs.Share;
using Be.Models;
using Be.Repositories.ShareRepo;
using Be.Services.EmailService;

namespace Be.Services.ShareService
{
    public class ShareService : IShareService
    {
        private readonly IShareRepository _shareRepository;
        private readonly IEmailService _emailService;

        public ShareService(IShareRepository shareRepository, IEmailService emailService)
        {
            _shareRepository = shareRepository;
            _emailService = emailService;
        }

        public async Task<ShareDto> ShareAsync(CreateShareDto dto)
        {
            var entity = new Share
            {
                CampaignId = dto.CampaignId,
                AccountId = dto.AccountId,
                GuestName = dto.GuestName,
                ReceiverEmail = dto.ReceiverEmail,
                Platform = dto.Platform,
                SharedAt = DateTime.UtcNow
            };

            await _shareRepository.AddAsync(entity);
            await _shareRepository.SaveChangesAsync();

            // Gửi email nếu là Email
            if (dto.Platform.ToLower() == "email" && !string.IsNullOrWhiteSpace(dto.ReceiverEmail))
            {
                await _emailService.SendShareEmailAsync(dto.ReceiverEmail, dto.CampaignId);
            }

            return new ShareDto
            {
                ShareId = entity.ShareId,
                CampaignId = entity.CampaignId,
                GuestName = entity.GuestName,
                ReceiverEmail = entity.ReceiverEmail,
                Platform = entity.Platform,
                SharedAt = entity.SharedAt,
                AccountId = entity.AccountId,
                AccountName = entity.Account?.FullName ?? "",
                CampaignTitle = entity.Campaign?.Title ?? ""
            };
        }

        public async Task<IEnumerable<ShareDto>> GetAllAsync()
        {
            var shares = await _shareRepository.GetAllAsync();

            return shares.Select(s => new ShareDto
            {
                ShareId = s.ShareId,
                CampaignId = s.CampaignId,
                GuestName = s.GuestName,
                ReceiverEmail = s.ReceiverEmail,
                Platform = s.Platform,
                SharedAt = s.SharedAt,
                AccountId = s.AccountId,
                AccountName = s.Account?.FullName ?? "",
                CampaignTitle = s.Campaign?.Title ?? ""
            });
        }

        public async Task<ShareDto?> GetByIdAsync(int id)
        {
            var s = await _shareRepository.GetByIdAsync(id);
            if (s == null) return null;

            return new ShareDto
            {
                ShareId = s.ShareId,
                CampaignId = s.CampaignId,
                GuestName = s.GuestName,
                ReceiverEmail = s.ReceiverEmail,
                Platform = s.Platform,
                SharedAt = s.SharedAt,
                AccountId = s.AccountId,
                AccountName = s.Account?.FullName ?? "",
                CampaignTitle = s.Campaign?.Title ?? ""
            };
        }
    }
}
