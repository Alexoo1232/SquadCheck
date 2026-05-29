namespace SquadCheck.Models;

public class Attendance
{
    public int Id { get; set; }

    public int PracticeSessionId { get; set; }

    public int UserId { get; set; }

    public AttendanceStatus Status { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public string? Comment { get; private set; }

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

    private static string? FormatComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        return comment.Trim();
    }
}
