using System.ComponentModel.DataAnnotations;
using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Form data for marking attendance.
/// </summary>
public class MarkAttendanceViewModel
{
    [Required]
    public int PracticeSessionId { get; set; }

    [Required(ErrorMessage = "User is required.")]
    [Display(Name = "Member")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Attendance status is required.")]
    [Display(Name = "Attendance")]
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Going;

    [StringLength(200, ErrorMessage = "Comment cannot be longer than 200 characters.")]
    public string? Comment { get; set; }
}
