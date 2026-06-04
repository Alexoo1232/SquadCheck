using System.ComponentModel.DataAnnotations;

namespace SquadCheck.Models.ViewModels;

public class CreateGroupViewModel
{
    [Required(ErrorMessage = "Group name is required.")]
    [StringLength(80, ErrorMessage = "Group name cannot be longer than 80 characters.")]
    [Display(Name = "Group name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Description cannot be longer than 300 characters.")]
    public string Description { get; set; } = string.Empty;
}
