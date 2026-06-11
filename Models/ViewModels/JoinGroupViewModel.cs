using System.ComponentModel.DataAnnotations;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Form data for joining a group by code.
/// </summary>
public class JoinGroupViewModel
{
    [Required(ErrorMessage = "Join code is required.")]
    [StringLength(20, ErrorMessage = "Join code cannot be longer than 20 characters.")]
    [Display(Name = "Join code")]
    public string Code { get; set; } = string.Empty;
}
