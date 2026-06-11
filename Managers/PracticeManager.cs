using SquadCheck.Data;
using SquadCheck.Models;

namespace SquadCheck.Managers;

/// <summary>
/// Works with practice sessions.
/// </summary>
public class PracticeManager
{
    private readonly IDataManager _dataManager;

    /// <summary>
    /// Gets the data manager used for storage.
    /// </summary>
    /// <param name="dataManager">The data manager used to load and save data.</param>
    public PracticeManager(IDataManager dataManager)
    {
        _dataManager = dataManager;
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
        AppData data = _dataManager.LoadData();
        Group? group = data.Groups.FirstOrDefault(group => group.Id == groupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        PracticeSession session = new PracticeSession(
            data.GetNextPracticeSessionId(),
            group.Id,
            title,
            startTime,
            endTime,
            location,
            maxParticipants);

        data.PracticeSessions.Add(session);

        _dataManager.SaveData(data);

        return session;
    }

    public PracticeSession? FindSessionById(int sessionId)
    {
        AppData data = _dataManager.LoadData();

        return data.PracticeSessions.FirstOrDefault(session => session.Id == sessionId);
    }

    public List<PracticeSession> GetAllSessions()
    {
        AppData data = _dataManager.LoadData();

        return data.PracticeSessions
            .OrderBy(session => session.StartTime)
            .ToList();
    }

    public List<PracticeSession> GetUpcomingSessions()
    {
        AppData data = _dataManager.LoadData();

        return data.PracticeSessions
            .Where(session => session.IsUpcoming())
            .OrderBy(session => session.StartTime)
            .ToList();
    }

    public List<PracticeSession> GetSessionsForGroup(int groupId)
    {
        AppData data = _dataManager.LoadData();
        bool groupExists = data.Groups.Any(group => group.Id == groupId);

        if (!groupExists)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        return data.PracticeSessions
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
        AppData data = _dataManager.LoadData();
        bool userExists = data.Users.Any(user => user.Id == userId);

        if (!userExists)
        {
            throw new InvalidOperationException("The user could not be found.");
        }

        List<int> groupIds = data.Groups
            .Where(group => group.HasMember(userId))
            .Select(group => group.Id)
            .ToList();

        return data.PracticeSessions
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
        AppData data = _dataManager.LoadData();
        PracticeSession? session = data.PracticeSessions.FirstOrDefault(session => session.Id == sessionId);

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

        _dataManager.SaveData(data);

        return session;
    }

    /// <summary>
    /// Cancels a practice and removes its attendance answers.
    /// </summary>
    /// <param name="sessionId">The id of the practice session to cancel.</param>
    /// <returns>True if the session was cancelled, otherwise false.</returns>
    public bool CancelSession(int sessionId)
    {
        AppData data = _dataManager.LoadData();
        PracticeSession? session = data.PracticeSessions.FirstOrDefault(session => session.Id == sessionId);

        if (session == null)
        {
            return false;
        }

        data.Attendances.RemoveAll(attendance => attendance.PracticeSessionId == sessionId);

        data.PracticeSessions.Remove(session);

        _dataManager.SaveData(data);

        return true;
    }
}
