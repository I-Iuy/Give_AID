using Be.DTOs.Share;
using Be.Models;
using Be.Repositories.ShareRepo;
using Be.Services.EmailService;
using System.Text.RegularExpressions;
using Be.Repositories.Campaigns;
using Microsoft.Extensions.Logging;

namespace Be.Services.ShareService
{
    public class ShareService : IShareService
    {
        private readonly IShareRepository _shareRepository;
        private readonly IEmailService _emailService;
        private readonly ICampaignRepository _campaignRepository;
        private readonly ILogger<ShareService> _logger;

        public ShareService(
            IShareRepository shareRepository,
            IEmailService emailService,
            ICampaignRepository campaignRepository,
            ILogger<ShareService> logger)
        {
            _shareRepository = shareRepository;
            _emailService = emailService;
            _campaignRepository = campaignRepository;
            _logger = logger;
        }

        public async Task<ShareDto> ShareAsync(CreateShareDto dto, string baseUrl)
        {
            try
            {
                // 1. Log thông tin bắt đầu chia sẻ
                _logger.LogInformation("[ShareService] Starting share process for campaign {CampaignId} on platform {Platform}",
                    dto.CampaignId, dto.Platform);

                // 2. Kiểm tra dto có null không
                if (dto == null)
                {
                    _logger.LogError("[ShareService] Share request is null");
                    throw new Exception("Share request cannot be null");
                }

                // 3. Kiểm tra chiến dịch có tồn tại không
                var campaign = await ValidateCampaign(dto.CampaignId.Value);
                _logger.LogInformation("[ShareService] Campaign validated: {CampaignTitle}", campaign.Title);

                // 4. Kiểm tra email nếu chia sẻ qua email
                if (IsEmailPlatform(dto.Platform))
                {
                    _logger.LogInformation("[ShareService] Validating email for platform: {Platform}", dto.Platform);
                    ValidateEmail(dto.ReceiverEmail);
                }

                // 5. Kiểm tra giới hạn số lần chia sẻ
                await CheckRateLimit(dto.AccountId, dto.GuestName);
                _logger.LogInformation("[ShareService] Rate limit check passed for user {AccountId}/{GuestName}",
                    dto.AccountId ?? 0, dto.GuestName ?? "anonymous");

                // 6. Tạo và lưu bản ghi chia sẻ
                var share = await CreateShareRecord(dto);
                _logger.LogInformation("[ShareService] Share record created with ID: {ShareId}", share.ShareId);

                // 7. Gửi email nếu cần
                if (IsEmailPlatform(dto.Platform))
                {
                    _logger.LogInformation("[ShareService] Sending email for platform: {Platform}", dto.Platform);
                    await SendShareEmail(dto, campaign, baseUrl);
                }
                else
                {
                    _logger.LogInformation("[ShareService] Skipping email send for platform: {Platform}", dto.Platform);
                }

                // 8. Trả về kết quả
                var result = MapToShareDto(share, campaign);
                _logger.LogInformation("[ShareService] Share process completed successfully for campaign {CampaignId}", dto.CampaignId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ShareService] Error sharing campaign {CampaignId}: {ErrorMessage}",
                    dto?.CampaignId, ex.Message);
                throw new Exception($"Failed to share campaign: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ShareDto>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("[ShareService] Getting all shares");
                var shares = await _shareRepository.GetAllAsync();
                var shareDtos = new List<ShareDto>();

                foreach (var share in shares)
                {
                    var campaign = await _campaignRepository.GetByIdAsync(share.CampaignId);
                    shareDtos.Add(MapToShareDto(share, campaign));
                }

                return shareDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ShareService] Error getting all shares");
                throw new Exception($"Error retrieving share list: {ex.Message}", ex);
            }
        }

        public async Task<ShareDto?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("[ShareService] Getting share by ID {Id}", id);
                var share = await _shareRepository.GetByIdAsync(id);

                if (share == null)
                {
                    _logger.LogWarning("[ShareService] Share with ID {Id} not found", id);
                    return null;
                }

                var campaign = await _campaignRepository.GetByIdAsync(share.CampaignId);
                return MapToShareDto(share, campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ShareService] Error getting share by ID {Id}", id);
                throw new Exception($"Error retrieving share information: {ex.Message}", ex);
            }
        }

        #region Private Methods

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private bool IsEmailPlatform(string platform)
        {
            return !string.IsNullOrEmpty(platform) && platform.ToLower() == "email";
        }

        private async Task<Campaign> ValidateCampaign(int campaignId)
        {
            _logger.LogInformation("[ShareService] Validating campaign {CampaignId}", campaignId);
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            if (campaign == null)
            {
                _logger.LogWarning("[ShareService] Campaign {CampaignId} not found", campaignId);
                throw new Exception("Campaign not found");
            }
            return campaign;
        }

        private void ValidateEmail(string? email)
        {
            _logger.LogInformation("[ShareService] Validating email for {Email}", email);

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("Please enter recipient's email address");
            }

            // Check if email is only whitespace
            if (email.Trim().Length == 0)
            {
                throw new Exception("Email address cannot contain only spaces");
            }

            // Basic email format validation
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, pattern))
            {
                throw new Exception("Invalid email address format");
            }

            // Check for common disposable email domains
            var disposableDomains = new[] {
                "tempmail.com", "throwawaymail.com", "mailinator.com",
                "guerrillamail.com", "10minutemail.com", "yopmail.com",
                "temp-mail.org", "sharklasers.com", "guerrillamail.info",
                "guerrillamail.biz", "guerrillamail.com", "guerrillamail.de",
                "guerrillamail.net", "guerrillamail.org", "guerrillamailblock.com",
                "spam4.me", "trashmail.com", "trashmail.me", "trashmail.net"
            };

            var domain = email.Split('@')[1].ToLower();
            if (disposableDomains.Contains(domain))
            {
                throw new Exception("Disposable email addresses are not allowed");
            }

            // Check for maximum length
            if (email.Length > 254) // RFC 5321
            {
                throw new Exception("Email address is too long");
            }

            // Check for consecutive dots
            if (email.Contains(".."))
            {
                throw new Exception("Email address contains invalid consecutive dots");
            }

            // Check for special characters in local part
            var localPart = email.Split('@')[0];
            if (Regex.IsMatch(localPart, @"[<>()[\]\\,;:\s""]"))
            {
                throw new Exception("Email address contains invalid characters");
            }

            // Check for domain format
            var domainPart = email.Split('@')[1];
            if (!Regex.IsMatch(domainPart, @"^[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9](?:\.[a-zA-Z]{2,})+$"))
            {
                throw new Exception("Invalid domain format in email address");
            }
        }

        private async Task CheckRateLimit(int? accountId, string? guestName)
        {
            _logger.LogInformation("[ShareService] Checking rate limit for user {AccountId}/{GuestName}",
                accountId ?? 0, guestName ?? "anonymous");

            var recentShares = await _shareRepository.GetRecentSharesAsync(accountId ?? 0, guestName);
            if (recentShares.Count() >= 5)
            {
                _logger.LogWarning("[ShareService] Rate limit exceeded for user {AccountId}/{GuestName}",
                    accountId ?? 0, guestName ?? "anonymous");
                throw new Exception("You have shared too many times in a short period. Please try again after 1 hour.");
            }

            _logger.LogInformation("[ShareService] Rate limiting check passed");
        }

        private async Task<Share> CreateShareRecord(CreateShareDto dto)
        {
            _logger.LogInformation("[ShareService] Creating share record for campaign {CampaignId}", dto.CampaignId);

            var entity = new Share
            {
                CampaignId = dto.CampaignId.Value,
                AccountId = dto.AccountId,
                GuestName = dto.GuestName,
                ReceiverEmail = dto.ReceiverEmail,
                Platform = dto.Platform,
                SharedAt = DateTime.UtcNow
            };

            _logger.LogInformation("[ShareService] Created share object: {@Share}", entity);
            await _shareRepository.AddAsync(entity);
            await _shareRepository.SaveChangesAsync();
            _logger.LogInformation("[ShareService] Share saved to database successfully with ID: {ShareId}", entity.ShareId);

            return entity;
        }

        private async Task SendShareEmail(CreateShareDto dto, Campaign campaign, string baseUrl)
        {
            _logger.LogInformation("[ShareService] Sending email to {Email}", dto.ReceiverEmail);

            var subject = $"Check out this campaign: {campaign.Title}";
            var body = $@"Hello,

{dto.GuestName} wants you to check out this campaign: {campaign.Title}

Click here to view: {_emailService.GetCampaignUrl(baseUrl, campaign.CampaignId)}

Best regards,
CharityHub Team";

            await _emailService.SendAsync(dto.ReceiverEmail, subject, body);
            _logger.LogInformation("[ShareService] Email sent successfully to {Email}", dto.ReceiverEmail);
        }

        private ShareDto MapToShareDto(Share share, Campaign? campaign = null)
        {
            return new ShareDto
            {
                ShareId = share.ShareId,
                CampaignId = share.CampaignId,
                CampaignTitle = campaign?.Title ?? share.Campaign?.Title ?? "",
                AccountId = share.AccountId,
                AccountName = share.Account?.FullName ?? "",
                GuestName = share.GuestName,
                ReceiverEmail = share.ReceiverEmail,
                Platform = share.Platform,
                SharedAt = share.SharedAt
            };
        }

        #endregion
    }
}