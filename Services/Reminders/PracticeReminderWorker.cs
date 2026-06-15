using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SquadCheck.Data;
using SquadCheck.Models;
using SquadCheck.Services.Email;

namespace SquadCheck.Services.Reminders;

/// <summary>
/// Sends due practice reminder emails in the background.
/// </summary>
public class PracticeReminderWorker : BackgroundService
{
    private const string PracticeReminderType = "PracticeReminder";
    private const string MissingResponseReminderType = "MissingResponseReminder";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PracticeReminderWorker> _logger;
    private readonly ReminderSettings _settings;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Gets the services used by the reminder worker.
    /// </summary>
    /// <param name="scopeFactory">Creates scoped services for each run.</param>
    /// <param name="logger">Writes reminder worker logs.</param>
    /// <param name="settings">The reminder settings.</param>
    /// <param name="configuration">Application configuration.</param>
    public PracticeReminderWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<PracticeReminderWorker> logger,
        IOptions<ReminderSettings> settings,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
        _configuration = configuration;
    }

    /// <summary>
    /// Runs the reminder loop.
    /// </summary>
    /// <param name="stoppingToken">Stops the worker when the app shuts down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Practice reminders are disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendDueRemindersAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while sending practice reminders.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(_settings.CheckIntervalMinutes),
                stoppingToken);
        }
    }

    /// <summary>
    /// Finds practice sessions that may need reminders.
    /// </summary>
    /// <param name="cancellationToken">Stops the query work when needed.</param>
    private async Task SendDueRemindersAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        IEmailService emailService =
            scope.ServiceProvider.GetRequiredService<IEmailService>();

        DateTime now = DateTime.Now;
        DateTime reminderLimit = now.AddHours(
            Math.Max(
                _settings.PracticeReminderHoursBefore,
                _settings.MissingResponseHoursBefore));

        List<PracticeSession> sessions = await context.PracticeSessions
            .Where(session =>
                session.StartTime > now &&
                session.StartTime <= reminderLimit)
            .OrderBy(session => session.StartTime)
            .ToListAsync(cancellationToken);

        foreach (PracticeSession session in sessions)
        {
            Group? group = await context.Groups
                .FirstOrDefaultAsync(group => group.Id == session.GroupId, cancellationToken);

            if (group == null)
            {
                continue;
            }

            List<GroupMember> members = await context.GroupMembers
                .Where(member => member.GroupId == group.Id)
                .ToListAsync(cancellationToken);

            foreach (GroupMember member in members)
            {
                User? user = await context.DomainUsers
                    .FirstOrDefaultAsync(user => user.Id == member.UserId, cancellationToken);

                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    continue;
                }

                await SendPracticeReminderIfDueAsync(
                    context,
                    emailService,
                    session,
                    group,
                    user,
                    now,
                    cancellationToken);

                await SendMissingResponseReminderIfDueAsync(
                    context,
                    emailService,
                    session,
                    group,
                    user,
                    now,
                    cancellationToken);
            }
        }
    }

    /// <summary>
    /// Sends the general practice reminder when it is due and has not already been sent.
    /// </summary>
    private async Task SendPracticeReminderIfDueAsync(
        ApplicationDbContext context,
        IEmailService emailService,
        PracticeSession session,
        Group group,
        User user,
        DateTime now,
        CancellationToken cancellationToken)
    {
        DateTime sendAfter = session.StartTime.AddHours(-_settings.PracticeReminderHoursBefore);

        if (now < sendAfter)
        {
            return;
        }

        bool alreadySent = await ReminderAlreadySentAsync(
            context,
            session.Id,
            user.Id,
            PracticeReminderType,
            cancellationToken);

        if (alreadySent)
        {
            return;
        }

        string subject = $"Practice reminder: {session.Title}";
        string body =
$"""
Hi {user.FullName},

This is a reminder for your upcoming practice.

Group: {group.Name}
Practice: {session.Title}
Date: {session.StartTime:yyyy-MM-dd}
Time: {session.StartTime:HH:mm} - {session.EndTime:HH:mm}
Location: {session.Location}

Open SquadCheck to check the details:
{BuildPracticeLink(session.Id)}

SquadCheck
""";

        await emailService.SendEmailAsync(user.Email, subject, body);

        await SaveReminderLogAsync(
            context,
            session.Id,
            user.Id,
            PracticeReminderType,
            cancellationToken);
    }

    /// <summary>
    /// Sends a missing-response reminder when the user has not answered attendance.
    /// </summary>
    private async Task SendMissingResponseReminderIfDueAsync(
        ApplicationDbContext context,
        IEmailService emailService,
        PracticeSession session,
        Group group,
        User user,
        DateTime now,
        CancellationToken cancellationToken)
    {
        DateTime sendAfter = session.StartTime.AddHours(-_settings.MissingResponseHoursBefore);

        if (now < sendAfter)
        {
            return;
        }

        bool hasAnswered = await context.Attendances.AnyAsync(attendance =>
            attendance.PracticeSessionId == session.Id &&
            attendance.UserId == user.Id &&
            attendance.Status != AttendanceStatus.NoResponse,
            cancellationToken);

        if (hasAnswered)
        {
            return;
        }

        bool alreadySent = await ReminderAlreadySentAsync(
            context,
            session.Id,
            user.Id,
            MissingResponseReminderType,
            cancellationToken);

        if (alreadySent)
        {
            return;
        }

        string subject = $"Please answer attendance: {session.Title}";
        string body =
$"""
Hi {user.FullName},

You have not answered attendance for this practice yet.

Group: {group.Name}
Practice: {session.Title}
Date: {session.StartTime:yyyy-MM-dd}
Time: {session.StartTime:HH:mm} - {session.EndTime:HH:mm}
Location: {session.Location}

Please open SquadCheck and choose Going, Maybe, or Not going:
{BuildPracticeLink(session.Id)}

SquadCheck
""";

        await emailService.SendEmailAsync(user.Email, subject, body);

        await SaveReminderLogAsync(
            context,
            session.Id,
            user.Id,
            MissingResponseReminderType,
            cancellationToken);
    }

    /// <summary>
    /// Checks whether a reminder was already sent for this user and practice.
    /// </summary>
    private async Task<bool> ReminderAlreadySentAsync(
        ApplicationDbContext context,
        int practiceSessionId,
        int userId,
        string notificationType,
        CancellationToken cancellationToken)
    {
        return await context.EmailNotificationLogs.AnyAsync(log =>
            log.PracticeSessionId == practiceSessionId &&
            log.UserId == userId &&
            log.NotificationType == notificationType,
            cancellationToken);
    }

    /// <summary>
    /// Saves a log entry after one reminder email has been sent.
    /// </summary>
    private async Task SaveReminderLogAsync(
        ApplicationDbContext context,
        int practiceSessionId,
        int userId,
        string notificationType,
        CancellationToken cancellationToken)
    {
        EmailNotificationLog log = new()
        {
            PracticeSessionId = practiceSessionId,
            UserId = userId,
            NotificationType = notificationType,
            SentAt = DateTime.Now
        };

        context.EmailNotificationLogs.Add(log);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Builds a practice-details link included in reminder emails.
    /// </summary>
    private string BuildPracticeLink(int practiceSessionId)
    {
        string baseUrl = _configuration["App:BaseUrl"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return $"/Practices/Details/{practiceSessionId}";
        }

        return $"{baseUrl.TrimEnd('/')}/Practices/Details/{practiceSessionId}";
    }
}
