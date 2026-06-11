namespace SquadCheck.Models;

/// <summary>
/// Stores one user's answer for one practice.
/// </summary>
public class Attendance
{
    public int Id { get; set; }

    public int PracticeSessionId { get; set; }

    public int UserId { get; set; }

    public AttendanceStatus Status { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// Creates an attendance answer.
    /// </summary>
    /// <param name="id">The attendance id.</param>
    /// <param name="practiceSessionId">The practice session id.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="status">The selected attendance status.</param>
    /// <param name="comment">An optional attendance comment.</param>
    public Attendance(
        int id,
        int practiceSessionId,
        int userId,
        AttendanceStatus status = AttendanceStatus.NoResponse,
        string? comment = null)
    {
        if (practiceSessionId <= 0)
        {
            throw new ArgumentException("Practice session id must be greater than zero.");
        }

        if (userId <= 0)
        {
            throw new ArgumentException("User id must be greater than zero.");
        }

        Id = id;
        PracticeSessionId = practiceSessionId;
        UserId = userId;
        Status = status;
        Comment = FormatComment(comment);
        UpdatedAt = DateTime.Now;
    }

    public Attendance()
    {
        Status = AttendanceStatus.NoResponse;
        UpdatedAt = DateTime.Now;
    }

    public void ChangeStatus(AttendanceStatus status, string? comment = null)
    {
        Status = status;
        Comment = FormatComment(comment);
        UpdatedAt = DateTime.Now;
    }

    public bool IsAnswered()
    {
        return Status != AttendanceStatus.NoResponse;
    }

    /// <summary>
    /// Trims a comment before saving it.
    /// </summary>
    /// <param name="comment">The comment text to clean.</param>
    /// <returns>The cleaned comment, or null when the comment is empty.</returns>
    private static string? FormatComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        return comment.Trim();
    }
}
