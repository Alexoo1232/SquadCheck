namespace SquadCheck.Models.ViewModels.Account;

/// <summary>
/// Data shown on the user's profile page.
/// </summary>
public class ProfileViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? DomainUserId { get; set; }

    public bool IsConnectedToDomainUser { get; set; }
}
