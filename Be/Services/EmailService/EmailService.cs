using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Be.Services.EmailService
{
    //=========================
    // EMAIL SERVICE - HANDLE SHARE EMAIL NOTIFICATIONS
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Method to send a share email to a recipient
        public async Task SendShareEmailAsync(string toEmail, int campaignId)
        {
            // 1. Lấy URL frontend từ cấu hình
            var frontendBaseUrl = _config["AppSettings:FrontendBaseUrl"] ?? "https://localhost:7108";
            
            // 2. Tạo tiêu đề và nội dung email
            var subject = "A campaign has been shared with you!";
            var body = $@"
                <p>You have received a shared campaign!</p>
                <p><a href='{frontendBaseUrl}/Web/Home/Post/{campaignId}'>View campaign</a></p>
            ";

            // 3. Gửi email
            await SendAsync(toEmail, subject, body);
        }

        public string GetCampaignUrl(string baseUrl, int campaignId)
        {
            return $"{baseUrl}/Web/Home/Post/{campaignId}";
        }

        // Method to send bulk emails to multiple recipients
        public async Task SendBulkEmailsAsync(IEnumerable<string> toEmails, string subject, string body, int? campaignId = null)
        {
            _logger.LogInformation("Starting to send bulk emails to {Count} recipients", toEmails.Count());

            // 1. Lấy URL frontend từ cấu hình
            var frontendBaseUrl = _config["AppSettings:FrontendBaseUrl"] ?? "https://localhost:7108";
            
            // 2. Tạo nội dung email
            var emailBody = $"<p>Hello,</p>" +
                          $"<p>You have a new notification:</p>" +
                          $"<h3>{subject}</h3>" +
                          $"<p>{body}</p>";

            // 3. Thêm link chiến dịch nếu có
            if (campaignId.HasValue)
            {
                emailBody += $"<p><a href='{frontendBaseUrl}/Web/Home/Post/{campaignId}'>View related campaign</a></p>";
            }

            emailBody += "<br/><p>Best regards,<br/>CharityHub Team</p>";

            // 4. Gửi email cho từng người nhận
            foreach (var email in toEmails)
            {
                try
                {
                    _logger.LogInformation("Sending email to {Email}", email);
                    await SendAsync(email, subject, emailBody);
                    _logger.LogInformation("Successfully sent email to {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email}", email);
                    // Continue with next email even if one fails
                }
            }

            _logger.LogInformation("Completed sending bulk emails");
        }

        // Generic method to send an email
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            try
            {
                // 1. Kiểm tra email người nhận
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    throw new ArgumentNullException(nameof(toEmail), "Recipient email address cannot be null or empty");
                }

                // 2. Lấy thông tin người gửi từ cấu hình
                var senderEmail = _config["EmailSettings:SenderEmail"];
                var senderName = _config["EmailSettings:SenderName"];
                var password = _config["EmailSettings:Password"];
                var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_config["EmailSettings:Port"] ?? "587");

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("Email configuration is missing. SenderEmail: {HasEmail}, Password: {HasPassword}", 
                        !string.IsNullOrEmpty(senderEmail), !string.IsNullOrEmpty(password));
                    throw new Exception("Email configuration is missing");
                }

                // 3. Tạo email message
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName ?? "CharityHub", senderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                // 4. Gửi email
                using var smtp = new SmtpClient();
                _logger.LogInformation("Connecting to SMTP server {Server}:{Port}", smtpServer, smtpPort);
                await smtp.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                
                _logger.LogInformation("Authenticating with SMTP server");
                await smtp.AuthenticateAsync(senderEmail, password);
                
                _logger.LogInformation("Sending email to {Email}", toEmail);
                await smtp.SendAsync(email);
                
                _logger.LogInformation("Disconnecting from SMTP server");
                await smtp.DisconnectAsync(true);
                
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                throw;
            }
        }
    }
    //=========================
}
