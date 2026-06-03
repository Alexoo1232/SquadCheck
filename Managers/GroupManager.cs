using SquadCheck.Data;
using SquadCheck.Models;

namespace SquadCheck.Managers;

public class GroupManager
{
    private readonly IDataManager _dataManager;

    public GroupManager(IDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public User CreateUser(string fullName, string email, bool isCoach = false)
    {
        AppData data = _dataManager.LoadData();

        bool emailAlreadyExists = data.Users.Any(user =>
            user.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (emailAlreadyExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        User user = new User(
            data.GetNextUserId(),
            fullName,
            email,
            isCoach);

        data.Users.Add(user);
        _dataManager.SaveData(data);

        return user;
    }

    public List<User> GetAllUsers()
    {
        AppData data = _dataManager.LoadData();

        return data.Users
            .OrderBy(user => user.FullName)
            .ToList();
    }

    public User? FindUserById(int userId)
    {
        AppData data = _dataManager.LoadData();

        return data.Users.FirstOrDefault(user => user.Id == userId);
    }

    public Group CreateGroup(string name, string description = "")
    {
        AppData data = _dataManager.LoadData();

        bool groupAlreadyExists = data.Groups.Any(group =>
            group.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (groupAlreadyExists)
        {
            throw new InvalidOperationException("A group with this name already exists.");
        }

        Group group = new Group(
            data.GetNextGroupId(),
            name,
            description);

        data.Groups.Add(group);
        _dataManager.SaveData(data);

        return group;
    }

    public List<Group> GetAllGroups()
    {
        AppData data = _dataManager.LoadData();

        return data.Groups
            .OrderBy(group => group.Name)
            .ToList();
    }

    public Group? FindGroupById(int groupId)
    {
        AppData data = _dataManager.LoadData();

        return data.Groups.FirstOrDefault(group => group.Id == groupId);
    }

    public Group UpdateGroup(int groupId, string name, string description)
    {
        AppData data = _dataManager.LoadData();

        Group group = data.Groups.FirstOrDefault(group => group.Id == groupId)
            ?? throw new InvalidOperationException("The group could not be found.");

        bool nameUsedByAnotherGroup = data.Groups.Any(existingGroup =>
            existingGroup.Id != groupId &&
            existingGroup.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (nameUsedByAnotherGroup)
        {
            throw new InvalidOperationException("Another group already uses this name.");
        }

        group.UpdateGroupDetails(name, description);

        _dataManager.SaveData(data);

        return group;
    }

    public bool DeleteGroup(int groupId)
    {
        AppData data = _dataManager.LoadData();

        Group? group = data.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            return false;
        }

        List<int> sessionIds = data.PracticeSessions
            .Where(session => session.GroupId == groupId)
            .Select(session => session.Id)
            .ToList();

        data.Attendances.RemoveAll(attendance =>
            sessionIds.Contains(attendance.PracticeSessionId));

        data.PracticeSessions.RemoveAll(session =>
            session.GroupId == groupId);

        data.Groups.Remove(group);

        _dataManager.SaveData(data);

        return true;
    }

    public GroupMember AddMemberToGroup(int groupId, int userId, GroupRole role = GroupRole.Member)
    {
        AppData data = _dataManager.LoadData();

        Group group = data.Groups.FirstOrDefault(group => group.Id == groupId)
            ?? throw new InvalidOperationException("The group could not be found.");

        User user = data.Users.FirstOrDefault(user => user.Id == userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (group.HasMember(user.Id))
        {
            throw new InvalidOperationException("This user is already a member of the group.");
        }

        GroupMember member = new GroupMember(
            GetNextGroupMemberId(data),
            group.Id,
            user.Id,
            role);

        group.AddMember(member);

        if (role == GroupRole.Coach)
        {
            user.SetCoachStatus(true);
        }

        _dataManager.SaveData(data);

        return member;
    }

    public bool RemoveMemberFromGroup(int groupId, int userId)
    {
        AppData data = _dataManager.LoadData();

        Group? group = data.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            return false;
        }

        bool removed = group.RemoveMember(userId);

        if (!removed)
        {
            return false;
        }

        List<int> groupSessionIds = data.PracticeSessions
            .Where(session => session.GroupId == groupId)
            .Select(session => session.Id)
            .ToList();

        data.Attendances.RemoveAll(attendance =>
            attendance.UserId == userId &&
            groupSessionIds.Contains(attendance.PracticeSessionId));

        foreach (PracticeSession session in data.PracticeSessions.Where(session => session.GroupId == groupId))
        {
            session.Attendances.RemoveAll(attendance => attendance.UserId == userId);
        }

        _dataManager.SaveData(data);

        return true;
    }

    public List<GroupMember> GetMembersForGroup(int groupId)
    {
        AppData data = _dataManager.LoadData();

        Group group = data.Groups.FirstOrDefault(group => group.Id == groupId)
            ?? throw new InvalidOperationException("The group could not be found.");

        return group.Members
            .OrderBy(member => member.Role)
            .ThenBy(member => member.UserId)
            .ToList();
    }

    private static int GetNextGroupMemberId(AppData data)
    {
        List<GroupMember> allMembers = data.Groups
            .SelectMany(group => group.Members)
            .ToList();

        if (allMembers.Count == 0)
        {
            return 1;
        }

        return allMembers.Max(member => member.Id) + 1;
    }
}
