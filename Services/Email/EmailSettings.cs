namespace SquadCheck.Services.Email;

/// <summary>
/// SMTP settings used when sending emails.
/// </summary>
public class EmailSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "SquadCheck";

    public bool EnableSsl { get; set; } = true;
}
