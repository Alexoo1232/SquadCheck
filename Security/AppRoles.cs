namespace SquadCheck.Security;

/// <summary>
/// Stores the application role names in one place.
/// </summary>
public static class AppRoles
{
    public const string Admin = "Admin";

    public const string Coach = "Coach";

    public const string Member = "Member";

    public const string AdminOrCoach = Admin + "," + Coach;
}
