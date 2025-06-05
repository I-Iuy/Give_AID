using System.Threading.Tasks;

namespace Be.Services.EmailService
{
    public interface IEmailService
    {
        Task SendShareEmailAsync(string toEmail, int campaignId);
        Task SendAsync(string toEmail, string subject, string body);
        string GetCampaignUrl(string baseUrl, int campaignId);
        Task SendNotificationEmailAsync(string toEmail, string title, string message, int? campaignId = null);
    }
}
