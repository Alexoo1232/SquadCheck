namespace SquadCheck.Models;

/// <summary>
/// Stores information about a sports group.
/// </summary>
public class Group
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public List<GroupMember> Members { get; set; }

    /// <summary>
    /// Creates a group with an empty member list.
    /// </summary>
    /// <param name="id">The group id.</param>
    /// <param name="name">The group name.</param>
    /// <param name="description">The optional group description.</param>
    public Group(int id, string name, string description = "")
    {
        Id = id;
        Name = ValidateName(name);
        Description = description.Trim();
        Members = new List<GroupMember>();
    }

    public Group()
    {
        Name = string.Empty;
        Description = string.Empty;
        Members = new List<GroupMember>();
    }

    public void UpdateGroupDetails(string name, string description)
    {
        Name = ValidateName(name);
        Description = description.Trim();
    }

    public void AddMember(GroupMember member)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        bool alreadyExists = Members.Any(existingMember => existingMember.UserId == member.UserId);

        if (alreadyExists)
        {
            throw new InvalidOperationException("This user is already a member of the group.");
        }

        Members.Add(member);
    }

    public bool RemoveMember(int userId)
    {
        GroupMember? member = Members.FirstOrDefault(member => member.UserId == userId);

        if (member == null)
        {
            return false;
        }

        Members.Remove(member);
        return true;
    }

    public bool HasMember(int userId)
    {
        return Members.Any(member => member.UserId == userId);
    }

    /// <summary>
    /// Checks and trims a group name.
    /// </summary>
    /// <param name="name">The group name to validate.</param>
    /// <returns>The trimmed group name.</returns>
    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Group name cannot be empty.");
        }

        return name.Trim();
    }
}
