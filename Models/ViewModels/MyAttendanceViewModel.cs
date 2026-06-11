using System.ComponentModel.DataAnnotations;
using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Form data for a logged-in user marking their own attendance.
/// </summary>
public class MyAttendanceViewModel
{
    [Required]
    public int PracticeSessionId { get; set; }

    [Required]
    public AttendanceStatus Status { get; set; }

    [StringLength(200)]
    public string? Comment { get; set; }
}
