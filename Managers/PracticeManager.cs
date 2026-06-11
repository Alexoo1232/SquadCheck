using SquadCheck.Data;
using SquadCheck.Models;
using Microsoft.EntityFrameworkCore;

namespace SquadCheck.Managers;

/// <summary>
/// Works with practice sessions.
/// </summary>
public class PracticeManager
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Gets the database context used for storage.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public PracticeManager(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new practice for a group.
    /// </summary>
    /// <param name="groupId">The id of the group that owns the session.</param>
    /// <param name="title">The practice title.</param>
    /// <param name="startTime">The practice start time.</param>
    /// <param name="endTime">The practice end time.</param>
    /// <param name="location">The practice location.</param>
    /// <param name="maxParticipants">The optional maximum number of going participants.</param>
    /// <returns>The created practice session.</returns>
    public PracticeSession CreateSession(
        int groupId,
        string title,
        DateTime startTime,
        DateTime endTime,
        string location,
        int? maxParticipants = null)
    {
        Group? group = _context.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        PracticeSession session = new PracticeSession(
            0,
            group.Id,
            title,
            startTime,
            endTime,
            location,
            maxParticipants);

        _context.PracticeSessions.Add(session);
        _context.SaveChanges();

        return session;
    }

    public PracticeSession? FindSessionById(int sessionId)
    {
        return _context.PracticeSessions.FirstOrDefault(session => session.Id == sessionId);
    }

    public List<PracticeSession> GetAllSessions()
    {
        return _context.PracticeSessions
            .OrderBy(session => session.StartTime)
            .ToList();
    }

    public List<PracticeSession> GetUpcomingSessions()
    {
        DateTime now = DateTime.Now;

        return _context.PracticeSessions
            .Where(session => session.StartTime > now)
            .OrderBy(session => session.StartTime)
            .ToList();
    }

    public List<PracticeSession> GetSessionsForGroup(int groupId)
    {
        bool groupExists = _context.Groups.Any(group => group.Id == groupId);

        if (!groupExists)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        return _context.PracticeSessions
            .Where(session => session.GroupId == groupId)
            .OrderBy(session => session.StartTime)
            .ToList();
    }

    /// <summary>
    /// Gets the practices a user can see through their groups.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The user's practice sessions sorted by start time.</returns>
    public List<PracticeSession> GetSessionsForUser(int userId)
    {
        bool userExists = _context.DomainUsers.Any(user => user.Id == userId);

        if (!userExists)
        {
            throw new InvalidOperationException("The user could not be found.");
        }

        List<int> groupIds = _context.GroupMembers
            .Where(member => member.UserId == userId)
            .Select(member => member.GroupId)
            .ToList();

        return _context.PracticeSessions
            .Where(session => groupIds.Contains(session.GroupId))
            .OrderBy(session => session.StartTime)
            .ToList();
    }

    /// <summary>
    /// Changes the details of a practice.
    /// </summary>
    /// <param name="sessionId">The id of the practice session to update.</param>
    /// <param name="title">The updated title.</param>
    /// <param name="startTime">The updated start time.</param>
    /// <param name="endTime">The updated end time.</param>
    /// <param name="location">The updated location.</param>
    /// <param name="maxParticipants">The updated optional participant limit.</param>
    /// <returns>The updated practice session.</returns>
    public PracticeSession UpdateSession(
        int sessionId,
        string title,
        DateTime startTime,
        DateTime endTime,
        string location,
        int? maxParticipants = null)
    {
        PracticeSession? session = _context.PracticeSessions.FirstOrDefault(session => session.Id == sessionId);

        if (session == null)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

        session.UpdateSessionDetails(
            title,
            startTime,
            endTime,
            location,
            maxParticipants);

        _context.SaveChanges();

        return session;
    }

    /// <summary>
    /// Cancels a practice and removes its attendance answers.
    /// </summary>
    /// <param name="sessionId">The id of the practice session to cancel.</param>
    /// <returns>True if the session was cancelled, otherwise false.</returns>
    public bool CancelSession(int sessionId)
    {
        PracticeSession? session = _context.PracticeSessions.FirstOrDefault(session => session.Id == sessionId);

        if (session == null)
        {
            return false;
        }

        _context.PracticeSessions.Remove(session);
        _context.SaveChanges();

        return true;
    }
}
