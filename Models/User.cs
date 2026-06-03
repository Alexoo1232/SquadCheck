namespace SquadCheck.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public bool IsCoach { get; set; }

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

    private static string ValidateText(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} cannot be empty.");
        }

        return value.Trim();
    }
}
