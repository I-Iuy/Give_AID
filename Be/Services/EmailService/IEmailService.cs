using System.Threading.Tasks;
using System.Collections.Generic;

namespace Be.Services.EmailService
{
    public interface IEmailService
    {
        Task SendShareEmailAsync(string toEmail, int campaignId);
        Task SendAsync(string toEmail, string subject, string body);
        string GetCampaignUrl(string baseUrl, int campaignId);
        Task SendBulkEmailsAsync(IEnumerable<string> toEmails, string subject, string body, int? campaignId = null);
    }
}
