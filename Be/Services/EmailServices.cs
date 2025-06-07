using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

public class EmailServices
{
    private readonly IConfiguration _config;

    public EmailServices(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendCustomEmail(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _config["EmailSettings:SenderName"],
            _config["EmailSettings:SenderEmail"]
        ));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = body };

        using var smtp = new SmtpClient();

        try
        {
            // ⚠️ DEV ONLY: Trust all certificates (fix SslHandshakeException)
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:Password"]
            );
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to send email. Please try again or check configuration.", ex);
        }
    }

    public async Task SendResetEmail(string toEmail, string resetLink)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _config["EmailSettings:SenderName"],
            _config["EmailSettings:SenderEmail"]
        ));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Reset your GiveAid password";
        email.Body = new TextPart("plain")
        {
            Text = $"Click the link below to reset your password:\n\n{resetLink}"
        };

        using var smtp = new SmtpClient();

        try
        {
            // ⚠️ DEV ONLY: Trust all certificates
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:Password"]
            );
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to send reset email. Please try again or check your email configuration.", ex);
        }
    }
}
