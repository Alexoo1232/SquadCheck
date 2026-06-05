using SquadCheck.Data;
using SquadCheck.Models;

namespace SquadCheck.Managers;

public class PracticeManager
{
    private readonly IDataManager _dataManager;

    public PracticeManager(IDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public PracticeSession CreateSession(
        int groupId,
        string title,
        DateTime startTime,
        DateTime endTime,
        string location,
        int? maxParticipants = null)
    {
        AppData data = _dataManager.LoadData();

        Group group = data.Groups.FirstOrDefault(group => group.Id == groupId)
            ?? throw new InvalidOperationException("The group could not be found.");

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

    public PracticeSession UpdateSession(
        int sessionId,
        string title,
        DateTime startTime,
        DateTime endTime,
        string location,
        int? maxParticipants = null)
    {
        AppData data = _dataManager.LoadData();

        PracticeSession session = data.PracticeSessions.FirstOrDefault(session => session.Id == sessionId)
            ?? throw new InvalidOperationException("The practice session could not be found.");

        session.UpdateSessionDetails(
            title,
            startTime,
            endTime,
            location,
            maxParticipants);

        _dataManager.SaveData(data);

        return session;
    }

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
