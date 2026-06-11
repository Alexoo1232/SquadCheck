using System.ComponentModel.DataAnnotations;
using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Form data for adding a member to a group.
/// </summary>
public class AddMemberViewModel
{
    [Required]
    public int GroupId { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, ErrorMessage = "Full name cannot be longer than 80 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(120, ErrorMessage = "Email cannot be longer than 120 characters.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public GroupRole Role { get; set; } = GroupRole.Member;
}
