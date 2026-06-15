namespace SquadCheck.Services.Reminders;

/// <summary>
/// Settings used by the background reminder worker.
/// </summary>
public class ReminderSettings
{
    public bool Enabled { get; set; } = false;

    public int CheckIntervalMinutes { get; set; } = 15;

    public int PracticeReminderHoursBefore { get; set; } = 24;

    public int MissingResponseHoursBefore { get; set; } = 12;
}
