namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Data shown for one user on the admin role page.
/// </summary>
public class AdminUserViewModel
{
    public string IdentityUserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? DomainUserId { get; set; }

    public bool IsDomainCoach { get; set; }

    public List<string> Roles { get; set; } = new();
}
