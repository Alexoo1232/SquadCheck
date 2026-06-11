using SquadCheck.Data;
using SquadCheck.Models;
using Microsoft.EntityFrameworkCore;

namespace SquadCheck.Managers;

/// <summary>
/// Works with attendance answers and summaries.
/// </summary>
public class AttendanceManager
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Gets the database context used for storage.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public AttendanceManager(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Saves or updates one attendance answer.
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
        PracticeSession? session = _context.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId);

        if (session == null)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

        ValidateMemberAccess(_context, practiceSessionId, userId);

        if (status == AttendanceStatus.Going)
        {
            int goingCountWithoutCurrentUser = _context.Attendances.Count(attendance =>
                attendance.PracticeSessionId == practiceSessionId &&
                attendance.UserId != userId &&
                attendance.Status == AttendanceStatus.Going);

            if (!session.CanAcceptGoing(goingCountWithoutCurrentUser))
            {
                throw new InvalidOperationException("The practice session is already full.");
            }
        }

        Attendance? existingAttendance = _context.Attendances.FirstOrDefault(attendance =>
            attendance.PracticeSessionId == practiceSessionId &&
            attendance.UserId == userId);

        if (existingAttendance == null)
        {
            Attendance attendance = new Attendance(
                0,
                practiceSessionId,
                userId,
                status,
                comment);

            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            return attendance;
        }

        existingAttendance.ChangeStatus(status, comment);

        _context.SaveChanges();

        return existingAttendance;
    }

    /// <summary>
    /// Gets all attendance records for a practice session.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <returns>The attendance records sorted by user id.</returns>
    public List<Attendance> GetAttendanceForSession(int practiceSessionId)
    {
        bool sessionExists = _context.PracticeSessions.Any(session => session.Id == practiceSessionId);

        if (!sessionExists)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

        return _context.Attendances
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
        bool userExists = _context.DomainUsers.Any(user => user.Id == userId);

        if (!userExists)
        {
            throw new InvalidOperationException("The user could not be found.");
        }

        return _context.Attendances
            .Where(attendance => attendance.UserId == userId)
            .OrderByDescending(attendance => attendance.UpdatedAt)
            .ToList();
    }

    /// <summary>
    /// Gets one user's answer for one practice session.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <returns>The attendance answer, or null.</returns>
    public Attendance? GetAttendanceForUserAndSession(int userId, int practiceSessionId)
    {
        return _context.Attendances.FirstOrDefault(attendance =>
            attendance.UserId == userId &&
            attendance.PracticeSessionId == practiceSessionId);
    }

    /// <summary>
    /// Counts the attendance answers for one practice.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <returns>A summary with going, not going, maybe, and no response counts.</returns>
    public AttendanceSummary GetSummary(int practiceSessionId)
    {
        PracticeSession? session = _context.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId);

        if (session == null)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

        bool groupExists = _context.Groups.Any(group => group.Id == session.GroupId);

        if (!groupExists)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        int goingCount = _context.Attendances.Count(attendance =>
            attendance.PracticeSessionId == practiceSessionId &&
            attendance.Status == AttendanceStatus.Going);

        int notGoingCount = _context.Attendances.Count(attendance =>
            attendance.PracticeSessionId == practiceSessionId &&
            attendance.Status == AttendanceStatus.NotGoing);

        int maybeCount = _context.Attendances.Count(attendance =>
            attendance.PracticeSessionId == practiceSessionId &&
            attendance.Status == AttendanceStatus.Maybe);

        int totalMembers = _context.GroupMembers.Count(member => member.GroupId == session.GroupId);
        int answeredCount = goingCount + notGoingCount + maybeCount;
        int noResponseCount = Math.Max(0, totalMembers - answeredCount);

        return new AttendanceSummary
        {
            PracticeSessionId = practiceSessionId,
            TotalMembers = totalMembers,
            GoingCount = goingCount,
            NotGoingCount = notGoingCount,
            MaybeCount = maybeCount,
            NoResponseCount = noResponseCount
        };
    }

    /// <summary>
    /// Checks if a user is allowed to access a practice.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <param name="userId">The id of the user.</param>
    /// <returns>True if the user can access the session, otherwise false.</returns>
    public bool CanUserAccessSession(int practiceSessionId, int userId)
    {
        PracticeSession? session = _context.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId);

        if (session == null)
        {
            return false;
        }

        return _context.GroupMembers.Any(member =>
            member.GroupId == session.GroupId &&
            member.UserId == userId);
    }

    /// <summary>
    /// Makes sure only group members can mark attendance.
    /// </summary>
    /// <param name="data">The loaded application data.</param>
    /// <param name="practiceSessionId">The id of the practice session.</param>
    /// <param name="userId">The id of the user.</param>
    private static void ValidateMemberAccess(ApplicationDbContext context, int practiceSessionId, int userId)
    {
        PracticeSession? session = context.PracticeSessions
            .FirstOrDefault(session => session.Id == practiceSessionId);

        if (session == null)
        {
            throw new InvalidOperationException("The practice session could not be found.");
        }

        bool groupExists = context.Groups.Any(group => group.Id == session.GroupId);

        if (!groupExists)
        {
            throw new InvalidOperationException("The group could not be found.");
        }

        bool userIsMember = context.GroupMembers.Any(member =>
            member.GroupId == session.GroupId &&
            member.UserId == userId);

        if (!userIsMember)
        {
            throw new InvalidOperationException("Only group members can mark attendance for this practice session.");
        }
    }
}
