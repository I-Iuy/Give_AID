using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

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
            var subject = "Chiến dịch được chia sẻ với bạn!";
            var body = $@"
                <p>Bạn vừa nhận được một chiến dịch được chia sẻ!</p>
                <p><a href='{frontendBaseUrl}/campaigns/{campaignId}'>Nhấn vào đây để xem chiến dịch</a></p>";

            await SendAsync(toEmail, subject, body);
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["EmailSettings:FromEmail"]
                ?? throw new InvalidOperationException("FromEmail config is missing");
            var password = _config["EmailSettings:Password"];
            var smtpHost = _config["EmailSettings:SmtpHost"];
            var smtpPortStr = _config["EmailSettings:SmtpPort"];
            var smtpPort = int.TryParse(smtpPortStr, out var port) ? port : 587;

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}
