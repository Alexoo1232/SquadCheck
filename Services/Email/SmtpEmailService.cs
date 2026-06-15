using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace SquadCheck.Services.Email;

/// <summary>
/// Sends emails through an SMTP server.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;

    /// <summary>
    /// Loads the configured SMTP settings.
    /// </summary>
    /// <param name="settings">The bound email settings.</param>
    public SmtpEmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Sends one email message.
    /// </summary>
    /// <param name="toEmail">The recipient email address.</param>
    /// <param name="subject">The message subject.</param>
    /// <param name="body">The plain-text message body.</param>
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("Email settings are not configured.");
        }

        using MailMessage message = new()
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using SmtpClient client = new(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            client.Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password);
        }

        await client.SendMailAsync(message);
    }
}
