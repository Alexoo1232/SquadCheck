namespace SquadCheck.Models;

/// <summary>
/// Stores information about one practice session.
/// </summary>
public class PracticeSession
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public string Title { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Location { get; set; }

    public int? MaxParticipants { get; set; }

    public Group? Group { get; set; }

    /// <summary>
    /// Creates a practice session.
    /// </summary>
    /// <param name="id">The practice session id.</param>
    /// <param name="groupId">The id of the group that owns the session.</param>
    /// <param name="title">The practice title.</param>
    /// <param name="startTime">The practice start time.</param>
    /// <param name="endTime">The practice end time.</param>
    /// <param name="location">The practice location.</param>
    /// <param name="maxParticipants">The optional maximum participant count.</param>
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

        SetTime(startTime, endTime);
    }

    public PracticeSession()
    {
        Title = string.Empty;
        Location = string.Empty;
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

    /// <summary>
    /// Checks if another member can be marked as going.
    /// </summary>
    /// <param name="currentGoingCount">The current number of going answers.</param>
    /// <returns>True if another going answer can be saved, otherwise false.</returns>
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

    /// <summary>
    /// Checks and sets the start and end times.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    private void SetTime(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("End time must be after start time.");
        }

        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Checks and trims required text.
    /// </summary>
    /// <param name="value">The text value to validate.</param>
    /// <param name="propertyName">The name used in the error message.</param>
    /// <returns>The trimmed text value.</returns>
    private static string ValidateText(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} cannot be empty.");
        }

        return value.Trim();
    }

    /// <summary>
    /// Checks the optional participant limit.
    /// </summary>
    /// <param name="maxParticipants">The participant limit to validate.</param>
    /// <returns>The validated participant limit.</returns>
    private static int? ValidateMaxParticipants(int? maxParticipants)
    {
        if (maxParticipants != null && maxParticipants <= 0)
        {
            throw new ArgumentException("Max participants must be greater than zero.");
        }

        return maxParticipants;
    }
}
