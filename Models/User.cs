using System.Text.Json.Serialization;

namespace SquadCheck.Models;

/// <summary>
/// Stores information about a user.
/// </summary>
public class User
{
    public int Id { get; set; }

    [JsonInclude]
    public string FullName { get; set; }

    [JsonInclude]
    public string Email { get; set; }

    [JsonInclude]
    public bool IsCoach { get; set; }

    [JsonInclude]
    public string? IdentityUserId { get; private set; }

    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="isCoach">Whether the user is a coach.</param>
    /// <param name="identityUserId">The linked ASP.NET Identity user id.</param>
    public User(
        int id,
        string fullName,
        string email,
        bool isCoach = false,
        string? identityUserId = null)
    {
        Id = id;
        FullName = ValidateText(fullName, nameof(fullName));
        Email = ValidateText(email, nameof(email));
        IsCoach = isCoach;
        IdentityUserId = string.IsNullOrWhiteSpace(identityUserId)
            ? null
            : identityUserId.Trim();
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
    /// Connects this domain user to an ASP.NET Identity user.
    /// </summary>
    /// <param name="identityUserId">The ASP.NET Identity user id.</param>
    public void ConnectIdentityUser(string identityUserId)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            throw new ArgumentException("Identity user id cannot be empty.");
        }

        IdentityUserId = identityUserId.Trim();
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
