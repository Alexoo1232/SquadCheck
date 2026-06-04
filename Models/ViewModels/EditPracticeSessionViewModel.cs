using System.ComponentModel.DataAnnotations;

namespace SquadCheck.Models.ViewModels;

public class EditPracticeSessionViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int GroupId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(80, ErrorMessage = "Title cannot be longer than 80 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start time is required.")]
    [Display(Name = "Start time")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    [Display(Name = "End time")]
    public DateTime EndTime { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(100, ErrorMessage = "Location cannot be longer than 100 characters.")]
    public string Location { get; set; } = string.Empty;

    [Range(1, 500, ErrorMessage = "Maximum participants must be between 1 and 500.")]
    [Display(Name = "Maximum participants")]
    public int? MaxParticipants { get; set; }
}
