using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

public class PracticeDetailsViewModel
{
    public PracticeSession PracticeSession { get; set; } = new();

    public Group Group { get; set; } = new();

    public List<User> Users { get; set; } = new();

    public List<GroupMember> Members { get; set; } = new();

    public List<Attendance> Attendances { get; set; } = new();

    public AttendanceSummary Summary { get; set; } = new();

    public MarkAttendanceViewModel MarkAttendance { get; set; } = new();
}
