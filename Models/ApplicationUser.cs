using Microsoft.AspNetCore.Identity;

namespace SquadCheck.Models;

/// <summary>
/// The ASP.NET Identity user for login and registration.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
