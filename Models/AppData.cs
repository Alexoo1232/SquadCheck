namespace SquadCheck.Models;

/// <summary>
/// Stores all data that is saved to and loaded from the JSON data file.
/// </summary>
public class AppData
{
    public List<User> Users { get; set; }

    public List<Group> Groups { get; set; }

    public List<PracticeSession> PracticeSessions { get; set; }

    public List<Attendance> Attendances { get; set; }

    public AppData()
    {
        Users = new List<User>();
        Groups = new List<Group>();
        PracticeSessions = new List<PracticeSession>();
        Attendances = new List<Attendance>();
    }

    public int GetNextUserId()
    {
        if (Users.Count == 0)
        {
            return 1;
        }

        return Users.Max(user => user.Id) + 1;
    }

    public int GetNextGroupId()
    {
        if (Groups.Count == 0)
        {
            return 1;
        }

        return Groups.Max(group => group.Id) + 1;
    }

    public int GetNextPracticeSessionId()
    {
        if (PracticeSessions.Count == 0)
        {
            return 1;
        }

        return PracticeSessions.Max(session => session.Id) + 1;
    }

    public int GetNextAttendanceId()
    {
        if (Attendances.Count == 0)
        {
            return 1;
        }

        return Attendances.Max(attendance => attendance.Id) + 1;
    }
}
