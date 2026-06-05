using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Contains all data needed by the practice details and attendance summary pages.
/// </summary>
public class PracticeDetailsViewModel
{
    public PracticeSession PracticeSession { get; set; } = new PracticeSession();

    public Group Group { get; set; } = new Group();

    public List<User> Users { get; set; } = new List<User>();

    public List<GroupMember> Members { get; set; } = new List<GroupMember>();

    public List<Attendance> Attendances { get; set; } = new List<Attendance>();

    public AttendanceSummary Summary { get; set; } = new AttendanceSummary();

    public MarkAttendanceViewModel MarkAttendance { get; set; } = new MarkAttendanceViewModel();
}
