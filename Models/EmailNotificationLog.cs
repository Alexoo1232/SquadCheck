namespace SquadCheck.Models;

/// <summary>
/// Stores one sent email reminder so duplicates are avoided.
/// </summary>
public class EmailNotificationLog
{
    public int Id { get; set; }

    public int PracticeSessionId { get; set; }

    public int UserId { get; set; }

    public string NotificationType { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.Now;

    public PracticeSession? PracticeSession { get; set; }

    public User? User { get; set; }
}
