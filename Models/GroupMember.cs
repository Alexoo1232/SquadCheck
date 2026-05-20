namespace SquadCheck.Models;

public class GroupMember
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int UserId { get; set; }

    public GroupRole Role { get; private set; }

    public DateTime JoinedAt { get; private set; }

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
