using SquadCheck.Data;
using SquadCheck.Models;

namespace SquadCheck.Managers;

/// <summary>
/// Handles users, groups, and group membership.
/// </summary>
public class GroupManager
{
    private readonly IDataManager _dataManager;

    /// <summary>
    /// Creates a manager that uses the shared data manager for storage.
    /// </summary>
    /// <param name="dataManager">The data manager used to load and save data.</param>
    public GroupManager(IDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    /// <summary>
    /// Creates a new user if the email is not already used.
    /// </summary>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="isCoach">Whether the user should be marked as a coach.</param>
    /// <returns>The created user.</returns>
    public User CreateUser(string fullName, string email, bool isCoach = false)
    {
        AppData data = _dataManager.LoadData();
        string cleanedEmail = email.Trim();
        bool emailAlreadyExists = data.Users.Any(user =>
            user.Email.Equals(cleanedEmail, StringComparison.OrdinalIgnoreCase));

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

    /// <summary>
    /// Creates a new group if the name is not already used.
    /// </summary>
    /// <param name="name">The group name.</param>
    /// <param name="description">The optional group description.</param>
    /// <returns>The created group.</returns>
    public Group CreateGroup(string name, string description = "")
    {
        AppData data = _dataManager.LoadData();
        string cleanedName = name.Trim();
        bool groupAlreadyExists = data.Groups.Any(group =>
            group.Name.Equals(cleanedName, StringComparison.OrdinalIgnoreCase));

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

    /// <summary>
    /// Updates the name and description of an existing group.
    /// </summary>
    /// <param name="groupId">The id of the group to update.</param>
    /// <param name="name">The new group name.</param>
    /// <param name="description">The new group description.</param>
    /// <returns>The updated group.</returns>
    public Group UpdateGroup(int groupId, string name, string description)
    {
        AppData data = _dataManager.LoadData();
        Group? group = data.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        string cleanedName = name.Trim();
        bool nameUsedByAnotherGroup = data.Groups.Any(existingGroup =>
            existingGroup.Id != groupId &&
            existingGroup.Name.Equals(cleanedName, StringComparison.OrdinalIgnoreCase));

        if (nameUsedByAnotherGroup)
        {
            throw new InvalidOperationException("Another group already uses this name.");
        }

        group.UpdateGroupDetails(name, description);

        _dataManager.SaveData(data);

        return group;
    }

    /// <summary>
    /// Deletes a group and removes practice and attendance data connected to it.
    /// </summary>
    /// <param name="groupId">The id of the group to delete.</param>
    /// <returns>True if the group was deleted, otherwise false.</returns>
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

        data.Attendances.RemoveAll(attendance => sessionIds.Contains(attendance.PracticeSessionId));
        data.PracticeSessions.RemoveAll(session => session.GroupId == groupId);

        data.Groups.Remove(group);

        _dataManager.SaveData(data);

        return true;
    }

    /// <summary>
    /// Adds an existing user to a group with the selected role.
    /// </summary>
    /// <param name="groupId">The id of the group.</param>
    /// <param name="userId">The id of the user.</param>
    /// <param name="role">The user's role in the group.</param>
    /// <returns>The created group membership.</returns>
    public GroupMember AddMemberToGroup(int groupId, int userId, GroupRole role = GroupRole.Member)
    {
        AppData data = _dataManager.LoadData();
        Group? group = data.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        User? user = data.Users.FirstOrDefault(user => user.Id == userId);

        if (user == null)
        {
            throw new InvalidOperationException("The user could not be found.");
        }

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

    /// <summary>
    /// Removes a user from a group and clears related attendance answers.
    /// </summary>
    /// <param name="groupId">The id of the group.</param>
    /// <param name="userId">The id of the user to remove.</param>
    /// <returns>True if the member was removed, otherwise false.</returns>
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

        _dataManager.SaveData(data);

        return true;
    }

    public List<GroupMember> GetMembersForGroup(int groupId)
    {
        AppData data = _dataManager.LoadData();
        Group? group = data.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

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
