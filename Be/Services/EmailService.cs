using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
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
            // Ghi log chi tiết để kiểm tra
            throw new Exception("Unable to send reset email. Please try again or check your email configuration.", ex);
        }
    }
}
