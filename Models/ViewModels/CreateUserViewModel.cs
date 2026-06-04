using System.ComponentModel.DataAnnotations;

namespace SquadCheck.Models.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, ErrorMessage = "Full name cannot be longer than 80 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(120, ErrorMessage = "Email cannot be longer than 120 characters.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Coach")]
    public bool IsCoach { get; set; }
}
