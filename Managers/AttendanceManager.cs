using SquadCheck.Data;
using SquadCheck.Models;

namespace SquadCheck.Managers;

/// <summary>
/// Handles attendance rules and attendance summaries for practice sessions.
/// </summary>
public class AttendanceManager
{
    private readonly IDataManager _dataManager;

    /// <summary>
    /// Creates a manager that uses the shared data manager for storage.
    /// </summary>
    /// <param name="dataManager">The data manager used to load and save data.</param>
    public AttendanceManager(IDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    /// <summary>
    /// Creates or updates one user's attendance answer for a practice session.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <param name="userId">The id of the user marking attendance.</param>
    /// <param name="status">The attendance status to save.</param>
    /// <param name="comment">An optional comment from the user.</param>
    /// <returns>The created or updated attendance record.</returns>
    public Attendance MarkAttendance(
        int practiceSessionId,
        int userId,
        AttendanceStatus status,
        string? comment = null)
    {
        AppData data = _dataManager.LoadData();
        PracticeSession? session = data.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId);

        if (session == null)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

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

            _dataManager.SaveData(data);

            return attendance;
        }

        existingAttendance.ChangeStatus(status, comment);

        _dataManager.SaveData(data);

        return existingAttendance;
    }

    /// <summary>
    /// Gets all attendance records for a practice session.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <returns>The attendance records sorted by user id.</returns>
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

    /// <summary>
    /// Gets all attendance records made by one user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The user's attendance records sorted by newest update first.</returns>
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

    /// <summary>
    /// Calculates attendance totals for a practice session.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <returns>A summary with going, not going, maybe, and no response counts.</returns>
    public AttendanceSummary GetSummary(int practiceSessionId)
    {
        AppData data = _dataManager.LoadData();
        PracticeSession? session = data.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId);

        if (session == null)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

        Group? group = data.Groups
            .FirstOrDefault(group => group.Id == session.GroupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        int goingCount = data.Attendances.Count(attendance =>
            attendance.PracticeSessionId == practiceSessionId &&
            attendance.Status == AttendanceStatus.Going);

        int notGoingCount = data.Attendances.Count(attendance =>
            attendance.PracticeSessionId == practiceSessionId &&
            attendance.Status == AttendanceStatus.NotGoing);

        int maybeCount = data.Attendances.Count(attendance =>
            attendance.PracticeSessionId == practiceSessionId &&
            attendance.Status == AttendanceStatus.Maybe);

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

    /// <summary>
    /// Checks whether a user belongs to the group connected to a practice session.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>True if the user can access the session, otherwise false.</returns>
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

    /// <summary>
    /// Stops users outside the group from marking attendance.
    /// </summary>
    /// <param name="data">The loaded application data.</param>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <param name="userId">The id of the user.</param>
    private static void ValidateMemberAccess(AppData data, int practiceSessionId, int userId)
    {
        PracticeSession? session = data.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId);

        if (session == null)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

        Group? group = data.Groups.FirstOrDefault(group => group.Id == session.GroupId);

        if (group == null)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        bool userIsMember = group.HasMember(userId);

        if (!userIsMember)
        {
            throw new InvalidOperationException("Only group members can mark attendance for this practice session.");
        }
    }
}
