using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Be.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // Method to send a share email to a recipient
        public async Task SendShareEmailAsync(string toEmail, int campaignId)
        {
            var frontendBaseUrl = _config["AppSettings:FrontendBaseUrl"] ?? "https://localhost:7108";
            var subject = "A campaign has been shared with you!";
            var body = $@"
                <p>You have received a shared campaign!</p>
                <p><a href='{frontendBaseUrl}/Web/Home/Post/{campaignId}'>View campaign</a></p>
            ";

            await SendAsync(toEmail, subject, body);
        }

        public string GetCampaignUrl(string baseUrl, int campaignId)
        {
            return $"{baseUrl}/Web/Home/Post/{campaignId}";
        }

        // Method to send a general notification email
        public async Task SendNotificationEmailAsync(string toEmail, string title, string message, int? campaignId = null)
        {
            var frontendBaseUrl = _config["AppSettings:FrontendBaseUrl"] ?? "https://localhost:7108";
            
            var body = $"<p>Hello,</p>" +
                       $"<p>You have a new notification:</p>" +
                       $"<h3>{title}</h3>" +
                       $"<p>{message}</p>";

            if (campaignId.HasValue)
            {
                body += $"<p><a href='{frontendBaseUrl}/Web/Home/Post/{campaignId}'>View related campaign</a></p>";
            }

            body += "<br/><p>Best regards,<br/>CharityHub Team</p>";

            await SendAsync(toEmail, title, body);
        }

        // Generic method to send an email
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _config["EmailSettings:SenderName"],
                    _config["EmailSettings:From"]));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _config["EmailSettings:SmtpServer"],
                    int.Parse(_config["EmailSettings:Port"]),
                    MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"]);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending email: {ex.Message}", ex);
            }
        }
    }
}
