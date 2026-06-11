namespace SquadCheck.Models;

/// <summary>
/// Holds the attendance totals for one practice.
/// </summary>
public class AttendanceSummary
{
    public int PracticeSessionId { get; set; }

    public int TotalMembers { get; set; }

    public int GoingCount { get; set; }

    public int NotGoingCount { get; set; }

    public int MaybeCount { get; set; }

    public int NoResponseCount { get; set; }

    public int AnsweredCount
    {
        get
        {
            return GoingCount + NotGoingCount + MaybeCount;
        }
    }
}
