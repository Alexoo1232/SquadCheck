namespace SquadCheck.Models;

public class PracticeSession
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public string Title { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Location { get; set; }

    public int? MaxParticipants { get; set; }

    public List<Attendance> Attendances { get; set; }

    public PracticeSession(
        int id,
        int groupId,
        string title,
        DateTime startTime,
        DateTime endTime,
        string location,
        int? maxParticipants = null)
    {
        if (groupId <= 0)
        {
            throw new ArgumentException("Group id must be greater than zero.");
        }

        Id = id;
        GroupId = groupId;
        Title = ValidateText(title, nameof(title));
        Location = ValidateText(location, nameof(location));
        MaxParticipants = ValidateMaxParticipants(maxParticipants);
        Attendances = new List<Attendance>();

        SetTime(startTime, endTime);
    }

    public PracticeSession()
    {
        Title = string.Empty;
        Location = string.Empty;
        Attendances = new List<Attendance>();
    }

    public void UpdateSessionDetails(
        string title,
        DateTime startTime,
        DateTime endTime,
        string location,
        int? maxParticipants = null)
    {
        Title = ValidateText(title, nameof(title));
        Location = ValidateText(location, nameof(location));
        MaxParticipants = ValidateMaxParticipants(maxParticipants);

        SetTime(startTime, endTime);
    }

    public bool IsUpcoming()
    {
        return StartTime > DateTime.Now;
    }

    public bool CanAcceptGoing(int currentGoingCount)
    {
        if (currentGoingCount < 0)
        {
            throw new ArgumentException("Current going count cannot be negative.");
        }

        if (MaxParticipants == null)
        {
            return true;
        }

        return currentGoingCount < MaxParticipants.Value;
    }

    public void AddOrUpdateAttendance(Attendance attendance)
    {
        if (attendance == null)
        {
            throw new ArgumentNullException(nameof(attendance));
        }

        Attendance? existingAttendance = Attendances
            .FirstOrDefault(item => item.UserId == attendance.UserId);

        if (existingAttendance == null)
        {
            Attendances.Add(attendance);
        }
        else
        {
            existingAttendance.ChangeStatus(attendance.Status, attendance.Comment);
        }
    }

    private void SetTime(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be after start time.");
        }

        StartTime = startTime;
        EndTime = endTime;
    }

    private static string ValidateText(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} cannot be empty.");
        }

        return value.Trim();
    }

    private static int? ValidateMaxParticipants(int? maxParticipants)
    {
        if (maxParticipants != null && maxParticipants <= 0)
        {
            throw new ArgumentException("Max participants must be greater than zero.");
        }

        return maxParticipants;
    }
}
