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

    // Sends a password reset email with a clickable link to the user
    public async Task SendResetEmail(string toEmail, string resetLink)
    {
        var email = new MimeMessage();

        // Set sender info from appsettings.json (EmailSettings section)
        email.From.Add(new MailboxAddress(
            _config["EmailSettings:SenderName"],
            _config["EmailSettings:SenderEmail"]
        ));

        // Set recipient info
        email.To.Add(MailboxAddress.Parse(toEmail));

        // Email subject and plain text body
        email.Subject = "Reset your GiveAid password";
        email.Body = new TextPart("plain")
        {
            Text = $"Click the link below to reset your password:\n\n{resetLink}"
        };

        // Initialize SMTP client
        using var smtp = new SmtpClient();

        try
        {
            // Connect to SMTP server (e.g., Gmail)
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

            // Authenticate with credentials from config
            await smtp.AuthenticateAsync(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:Password"]
            );

            // Send email
            await smtp.SendAsync(email);

            // Disconnect cleanly
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Optionally log the error or handle failure
            throw new Exception("Unable to send reset email. Please try again or check your email configuration.", ex);
        }
    }
}
