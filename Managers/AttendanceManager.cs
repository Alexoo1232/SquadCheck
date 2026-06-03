using SquadCheck.Data;
using SquadCheck.Models;

namespace SquadCheck.Managers;

public class AttendanceManager
{
    private readonly IDataManager _dataManager;

    public AttendanceManager(IDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public Attendance MarkAttendance(
        int practiceSessionId,
        int userId,
        AttendanceStatus status,
        string? comment = null)
    {
        AppData data = _dataManager.LoadData();

        PracticeSession session = data.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId)
            ?? throw new InvalidOperationException("The practice session could not be found.");

        ValidateMemberAccess(data, practiceSessionId, userId);

        if (status == AttendanceStatus.Going)
        {
            int goingCountWithoutCurrentUser = data.Attendances.Count(attendance =>
                attendance.PracticeSessionId == practiceSessionId &&
                attendance.UserId != userId &&
                attendance.Status == AttendanceStatus.Going);

            if (!session.CanAcceptGoing(goingCountWithoutCurrentUser))
            {
                throw new InvalidOperationException("The practice session is already full.");
            }
        }

        Attendance? existingAttendance = data.Attendances.FirstOrDefault(attendance =>
            attendance.PracticeSessionId == practiceSessionId &&
            attendance.UserId == userId);

        if (existingAttendance == null)
        {
            Attendance attendance = new Attendance(
                data.GetNextAttendanceId(),
                practiceSessionId,
                userId,
                status,
                comment);

            data.Attendances.Add(attendance);
            session.AddOrUpdateAttendance(attendance);

            SyncSessionAttendanceInsideGroup(data, session, attendance);

            _dataManager.SaveData(data);

            return attendance;
        }

        existingAttendance.ChangeStatus(status, comment);
        session.AddOrUpdateAttendance(existingAttendance);

        SyncSessionAttendanceInsideGroup(data, session, existingAttendance);

        _dataManager.SaveData(data);

        return existingAttendance;
    }

    public List<Attendance> GetAttendanceForSession(int practiceSessionId)
    {
        AppData data = _dataManager.LoadData();

        bool sessionExists = data.PracticeSessions.Any(session => session.Id == practiceSessionId);

        if (!sessionExists)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

        return data.Attendances
            .Where(attendance => attendance.PracticeSessionId == practiceSessionId)
            .OrderBy(attendance => attendance.UserId)
            .ToList();
    }

    public List<Attendance> GetAttendanceForUser(int userId)
    {
        AppData data = _dataManager.LoadData();

        bool userExists = data.Users.Any(user => user.Id == userId);

        if (!userExists)
        {
            throw new InvalidOperationException("The user could not be found.");
        }

        return data.Attendances
            .Where(attendance => attendance.UserId == userId)
            .OrderByDescending(attendance => attendance.UpdatedAt)
            .ToList();
    }

    public AttendanceSummary GetSummary(int practiceSessionId)
    {
        AppData data = _dataManager.LoadData();

        PracticeSession session = data.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId)
            ?? throw new InvalidOperationException("The practice session could not be found.");

        Group group = data.Groups
            .FirstOrDefault(group => group.Id == session.GroupId)
            ?? throw new InvalidOperationException("The group could not be found.");

        List<Attendance> sessionAttendances = data.Attendances
            .Where(attendance => attendance.PracticeSessionId == practiceSessionId)
            .ToList();

        int goingCount = sessionAttendances.Count(attendance => attendance.Status == AttendanceStatus.Going);
        int notGoingCount = sessionAttendances.Count(attendance => attendance.Status == AttendanceStatus.NotGoing);
        int maybeCount = sessionAttendances.Count(attendance => attendance.Status == AttendanceStatus.Maybe);

        int answeredCount = goingCount + notGoingCount + maybeCount;
        int noResponseCount = Math.Max(0, group.Members.Count - answeredCount);

        return new AttendanceSummary
        {
            PracticeSessionId = practiceSessionId,
            TotalMembers = group.Members.Count,
            GoingCount = goingCount,
            NotGoingCount = notGoingCount,
            MaybeCount = maybeCount,
            NoResponseCount = noResponseCount
        };
    }

    public bool CanUserAccessSession(int practiceSessionId, int userId)
    {
        AppData data = _dataManager.LoadData();

        PracticeSession? session = data.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId);

        if (session == null)
        {
            return false;
        }

        Group? group = data.Groups.FirstOrDefault(group => group.Id == session.GroupId);

        if (group == null)
        {
            return false;
        }

        return group.HasMember(userId);
    }

    private static void ValidateMemberAccess(AppData data, int practiceSessionId, int userId)
    {
        PracticeSession session = data.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId)
            ?? throw new InvalidOperationException("The practice session could not be found.");

        Group group = data.Groups
            .FirstOrDefault(group => group.Id == session.GroupId)
            ?? throw new InvalidOperationException("The group could not be found.");

        bool userIsMember = group.HasMember(userId);

        if (!userIsMember)
        {
            throw new InvalidOperationException("Only group members can mark attendance for this practice session.");
        }
    }

    private static void SyncSessionAttendanceInsideGroup(
        AppData data,
        PracticeSession session,
        Attendance attendance)
    {
        Group? group = data.Groups.FirstOrDefault(group => group.Id == session.GroupId);

        PracticeSession? groupSession = group?.PracticeSessions
            .FirstOrDefault(item => item.Id == session.Id);

        groupSession?.AddOrUpdateAttendance(attendance);
    }
}
