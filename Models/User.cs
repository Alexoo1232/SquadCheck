namespace SquadCheck.Models;

/// <summary>
/// Stores information about a user.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public bool IsCoach { get; set; }

    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="isCoach">Whether the user is a coach.</param>
    public User(int id, string fullName, string email, bool isCoach = false)
    {
        Id = id;
        FullName = ValidateText(fullName, nameof(fullName));
        Email = ValidateText(email, nameof(email));
        IsCoach = isCoach;
    }

    public User()
    {
        FullName = string.Empty;
        Email = string.Empty;
    }

    public void UpdateContactDetails(string fullName, string email)
    {
        FullName = ValidateText(fullName, nameof(fullName));
        Email = ValidateText(email, nameof(email));
    }

    public void SetCoachStatus(bool isCoach)
    {
        IsCoach = isCoach;
    }

    /// <summary>
    /// Checks if the user is a coach in a group.
    /// </summary>
    /// <param name="group">The group to check.</param>
    /// <returns>True if the user is a coach in the group, otherwise false.</returns>
    public bool IsLeaderOf(Group group)
    {
        if (group == null)
        {
            return false;
        }

        return group.Members.Any(member =>
            member.UserId == Id &&
            member.Role == GroupRole.Coach);
    }

    /// <summary>
    /// Checks and trims required text.
    /// </summary>
    /// <param name="value">The text value to validate.</param>
    /// <param name="propertyName">The name used in the error message.</param>
    /// <returns>The trimmed text value.</returns>
    private static string ValidateText(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} cannot be empty.");
        }

        return value.Trim();
    }
}
