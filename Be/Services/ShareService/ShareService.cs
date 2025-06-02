using Be.DTOs.Share;
using Be.Models;
using Be.Repositories.ShareRepo;
using Be.Services.EmailService;

using System.Text.RegularExpressions;
using Be.Repositories.Campaigns;

namespace Be.Services.ShareService
{
    public class ShareService : IShareService
    {
        private readonly IShareRepository _shareRepository;
        private readonly IEmailService _emailService;
        private readonly ICampaignRepository _campaignRepository;

        public ShareService(
            IShareRepository shareRepository, 
            IEmailService emailService,
            ICampaignRepository campaignRepository)
        {
            _shareRepository = shareRepository;
            _emailService = emailService;
            _campaignRepository = campaignRepository;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        public async Task<ShareDto> ShareAsync(CreateShareDto dto)
        {
            // Kiểm tra campaign tồn tại
            var campaign = await _campaignRepository.GetByIdAsync(dto.CampaignId);
            if (campaign == null)
            {
                throw new Exception("Campaign không tồn tại");
            }

            // Kiểm tra email nếu share qua email
            if (dto.Platform.ToLower() == "email" && !string.IsNullOrWhiteSpace(dto.ReceiverEmail))
            {
                if (!IsValidEmail(dto.ReceiverEmail))
                {
                    throw new Exception("Email người nhận không hợp lệ");
                }
            }

            // Kiểm tra rate limiting (tối đa 5 lần share trong 1 giờ)
            var recentShares = await _shareRepository.GetRecentSharesAsync(dto.AccountId ?? 0, dto.GuestName);
            if (recentShares.Count() >= 5)
            {
                throw new Exception("Bạn đã share quá nhiều trong thời gian ngắn. Vui lòng thử lại sau.");
            }

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
