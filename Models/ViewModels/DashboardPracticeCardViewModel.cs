using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Data shown for one practice card on the dashboard.
/// </summary>
public class DashboardPracticeCardViewModel
{
    public PracticeSession PracticeSession { get; set; } = new();

    public Group Group { get; set; } = new();

    public Attendance? CurrentUserAttendance { get; set; }

    public AttendanceSummary? Summary { get; set; }

    public bool CanManagePractice { get; set; }

    public bool IsNextPractice { get; set; }
}
