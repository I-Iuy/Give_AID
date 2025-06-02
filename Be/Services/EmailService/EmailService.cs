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

        public async Task SendShareEmailAsync(string toEmail, int campaignId)
        {
            var frontendBaseUrl = _config["AppSettings:FrontendBaseUrl"] ?? "https://localhost:3000";
            var subject = "A Campaign Has Been Shared With You!";
            var body = $@"
                <p>You have received a campaign that was shared with you!</p>
                <p><a href='{frontendBaseUrl}/campaign/{campaignId}'>View Campaign</a></p>
            ";

            await SendAsync(toEmail, subject, body);
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Charity App", _config["EmailSettings:From"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:Port"]),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _config["EmailSettings:Username"],
                _config["EmailSettings:Password"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
