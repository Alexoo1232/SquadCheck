namespace SquadCheck.Models;

/// <summary>
/// Connects a user to a group with a role.
/// </summary>
public class GroupMember
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int UserId { get; set; }

    public GroupRole Role { get; set; }

    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Creates a group membership.
    /// </summary>
    /// <param name="id">The membership id.</param>
    /// <param name="groupId">The group id.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="role">The user's role in the group.</param>
    public GroupMember(int id, int groupId, int userId, GroupRole role = GroupRole.Member)
    {
        if (groupId <= 0)
        {
            throw new ArgumentException("Group id must be greater than zero.");
        }

        if (userId <= 0)
        {
            throw new ArgumentException("User id must be greater than zero.");
        }

        Id = id;
        GroupId = groupId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.Now;
    }

    public GroupMember()
    {
        JoinedAt = DateTime.Now;
    }

    public void ChangeRole(GroupRole role)
    {
        Role = role;
    }

    public bool IsCoach()
    {
        return Role == GroupRole.Coach;
    }
}
