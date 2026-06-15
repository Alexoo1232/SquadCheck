using System.ComponentModel.DataAnnotations;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Form data for creating recurring practice sessions.
/// </summary>
public class CreateRecurringPracticeViewModel
{
    [Required(ErrorMessage = "Group is required.")]
    [Display(Name = "Group")]
    public int GroupId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(80)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Repeat until date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Repeat until")]
    public DateTime RepeatUntil { get; set; } = DateTime.Today.AddMonths(1);

    [Required(ErrorMessage = "Start time is required.")]
    [DataType(DataType.Time)]
    [Display(Name = "Start time")]
    public TimeSpan StartTime { get; set; } = new(18, 0, 0);

    [Required(ErrorMessage = "End time is required.")]
    [DataType(DataType.Time)]
    [Display(Name = "End time")]
    public TimeSpan EndTime { get; set; } = new(20, 0, 0);

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Range(1, 500)]
    [Display(Name = "Maximum participants")]
    public int? MaxParticipants { get; set; }

    public bool Monday { get; set; }

    public bool Tuesday { get; set; }

    public bool Wednesday { get; set; }

    public bool Thursday { get; set; }

    public bool Friday { get; set; }

    public bool Saturday { get; set; }

    public bool Sunday { get; set; }
}
