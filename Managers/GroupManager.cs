using SquadCheck.Data;
using SquadCheck.Models;
using Microsoft.EntityFrameworkCore;

namespace SquadCheck.Managers;

/// <summary>
/// Works with users, groups, and memberships.
/// </summary>
public class GroupManager
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Gets the database context used for storage.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public GroupManager(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a user if the email is available.
    /// </summary>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="isCoach">Whether the user should be marked as a coach.</param>
    /// <param name="identityUserId">The linked ASP.NET Identity user id.</param>
    /// <returns>The created user.</returns>
    public User CreateUser(
        string fullName,
        string email,
        bool isCoach = false,
        string? identityUserId = null)
    {
        string cleanedEmail = email.Trim();
        string normalizedEmail = cleanedEmail.ToLower();
        bool emailAlreadyExists = _context.DomainUsers.Any(user =>
            user.Email.ToLower() == normalizedEmail);

        if (emailAlreadyExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        User user = new User(
            0,
            fullName,
            email,
            isCoach,
            identityUserId);

        _context.DomainUsers.Add(user);
        _context.SaveChanges();

        return user;
    }

    /// <summary>
    /// Finds a domain user by linked ASP.NET Identity user id.
    /// </summary>
    /// <param name="identityUserId">The ASP.NET Identity user id.</param>
    /// <returns>The matching domain user, or null.</returns>
    public User? FindUserByIdentityUserId(string identityUserId)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            return null;
        }

        return _context.DomainUsers.FirstOrDefault(user =>
            user.IdentityUserId == identityUserId);
    }

    /// <summary>
    /// Creates or links a domain user to an ASP.NET Identity user.
    /// </summary>
    /// <param name="fullName">The user's full name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="identityUserId">The ASP.NET Identity user id.</param>
    /// <returns>The created or updated domain user.</returns>
    public User CreateOrConnectIdentityUser(
        string fullName,
        string email,
        string identityUserId)
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            throw new ArgumentException("Identity user id cannot be empty.");
        }

        string cleanedEmail = email.Trim();
        string normalizedEmail = cleanedEmail.ToLower();

        User? userByIdentity = _context.DomainUsers.FirstOrDefault(user =>
            user.IdentityUserId == identityUserId);

        if (userByIdentity != null)
        {
            userByIdentity.UpdateContactDetails(fullName, email);
            _context.SaveChanges();
            return userByIdentity;
        }

        User? userByEmail = _context.DomainUsers.FirstOrDefault(user =>
            user.Email.ToLower() == normalizedEmail);

        if (userByEmail != null)
        {
            userByEmail.UpdateContactDetails(fullName, email);
            userByEmail.ConnectIdentityUser(identityUserId);
            _context.SaveChanges();
            return userByEmail;
        }

        User newUser = new User(
            0,
            fullName,
            email,
            false,
            identityUserId);

        _context.DomainUsers.Add(newUser);
        _context.SaveChanges();

        return newUser;
    }

    public List<User> GetAllUsers()
    {
        return _context.DomainUsers
            .OrderBy(user => user.FullName)
            .ToList();
    }

    public User? FindUserById(int userId)
    {
        return _context.DomainUsers.FirstOrDefault(user => user.Id == userId);
    }

    /// <summary>
    /// Creates a group if the name is available.
    /// </summary>
    /// <param name="name">The group name.</param>
    /// <param name="description">The optional group description.</param>
    /// <returns>The created group.</returns>
    public Group CreateGroup(string name, string description = "")
    {
        string cleanedName = name.Trim();
        string normalizedName = cleanedName.ToLower();
        bool groupAlreadyExists = _context.Groups.Any(group =>
            group.Name.ToLower() == normalizedName);

        if (groupAlreadyExists)
        {
            throw new InvalidOperationException("A group with this name already exists.");
        }

        Group group = new Group(
            0,
            name,
            description);

        _context.Groups.Add(group);
        _context.SaveChanges();

        return group;
    }

    public List<Group> GetAllGroups()
    {
        return _context.Groups
            .Include(group => group.Members)
            .OrderBy(group => group.Name)
            .ToList();
    }

    public Group? FindGroupById(int groupId)
    {
        return _context.Groups
            .Include(group => group.Members)
            .FirstOrDefault(group => group.Id == groupId);
    }

    /// <summary>
    /// Changes a group's name and description.
    /// </summary>
    /// <param name="groupId">The id of the group to update.</param>
    /// <param name="name">The new group name.</param>
    /// <param name="description">The new group description.</param>
    /// <returns>The updated group.</returns>
    public Group UpdateGroup(int groupId, string name, string description)
    {
        Group? group = _context.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        string cleanedName = name.Trim();
        string normalizedName = cleanedName.ToLower();
        bool nameUsedByAnotherGroup = _context.Groups.Any(existingGroup =>
            existingGroup.Id != groupId &&
            existingGroup.Name.ToLower() == normalizedName);

        if (nameUsedByAnotherGroup)
        {
            throw new InvalidOperationException("Another group already uses this name.");
        }

        group.UpdateGroupDetails(name, description);

        _context.SaveChanges();

        return group;
    }

    /// <summary>
    /// Deletes a group and its related practice data.
    /// </summary>
    /// <param name="groupId">The id of the group to delete.</param>
    /// <returns>True if the group was deleted, otherwise false.</returns>
    public bool DeleteGroup(int groupId)
    {
        Group? group = _context.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            return false;
        }
        
        _context.Groups.Remove(group);
        _context.SaveChanges();

        return true;
    }

    /// <summary>
    /// Adds an existing user to a group.
    /// </summary>
    /// <param name="groupId">The id of the group.</param>
    /// <param name="userId">The id of the user.</param>
    /// <param name="role">The user's role in the group.</param>
    /// <returns>The created group membership.</returns>
    public GroupMember AddMemberToGroup(int groupId, int userId, GroupRole role = GroupRole.Member)
    {
        Group? group = _context.Groups
            .Include(existingGroup => existingGroup.Members)
            .FirstOrDefault(existingGroup => existingGroup.Id == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        User? user = _context.DomainUsers.FirstOrDefault(user => user.Id == userId);

        if (user == null)
        {
            throw new InvalidOperationException("The user could not be found.");
        }

        if (group.HasMember(user.Id))
        {
            throw new InvalidOperationException("This user is already a member of the group.");
        }

        GroupMember member = new GroupMember(
            0,
            group.Id,
            user.Id,
            role);

        _context.GroupMembers.Add(member);

        if (role == GroupRole.Coach)
        {
            user.SetCoachStatus(true);
        }

        _context.SaveChanges();

        return member;
    }

    /// <summary>
    /// Removes a user from a group.
    /// </summary>
    /// <param name="groupId">The id of the group.</param>
    /// <param name="userId">The id of the user to remove.</param>
    /// <returns>True if the member was removed, otherwise false.</returns>
    public bool RemoveMemberFromGroup(int groupId, int userId)
    {
        GroupMember? member = _context.GroupMembers.FirstOrDefault(member =>
            member.GroupId == groupId &&
            member.UserId == userId);

        if (member == null)
        {
            return false;
        }

        List<int> groupSessionIds = _context.PracticeSessions
            .Where(practiceSession => practiceSession.GroupId == groupId)
            .Select(practiceSession => practiceSession.Id)
            .ToList();

        List<Attendance> attendances = _context.Attendances.Where(attendance =>
            attendance.UserId == userId &&
            groupSessionIds.Contains(attendance.PracticeSessionId))
            .ToList();

        _context.Attendances.RemoveRange(attendances);
        _context.GroupMembers.Remove(member);
        _context.SaveChanges();

        return true;
    }

    public List<GroupMember> GetMembersForGroup(int groupId)
    {
        bool groupExists = _context.Groups.Any(group => group.Id == groupId);

        if (!groupExists)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        return _context.GroupMembers
            .Where(member => member.GroupId == groupId)
            .OrderBy(member => member.Role)
            .ThenBy(member => member.UserId)
            .ToList();
    }
}
