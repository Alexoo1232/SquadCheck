namespace SquadCheck.Services.Email;

/// <summary>
/// Sends email messages.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
