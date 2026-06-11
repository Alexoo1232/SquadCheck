using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;
using SquadCheck.Security;

using DomainUser = SquadCheck.Models.User;

namespace SquadCheck.Controllers;

/// <summary>
/// Controls the dashboard, landing page, and error pages.
/// </summary>
[Authorize]
public class HomeController : Controller
{
    private readonly GroupManager _groupManager;
    private readonly PracticeManager _practiceManager;
    private readonly AttendanceManager _attendanceManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Gets the managers used by the dashboard.
    /// </summary>
    /// <param name="groupManager">Manager used for groups and memberships.</param>
    /// <param name="practiceManager">Manager used for practice sessions.</param>
    /// <param name="attendanceManager">Manager used for attendance answers and summaries.</param>
    /// <param name="userManager">Manager used to read the current Identity user.</param>
    public HomeController(
        GroupManager groupManager,
        PracticeManager practiceManager,
        AttendanceManager attendanceManager,
        UserManager<ApplicationUser> userManager)
    {
        _groupManager = groupManager;
        _practiceManager = practiceManager;
        _attendanceManager = attendanceManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Shows the logged-in user's dashboard.
    /// </summary>
    /// <returns>The dashboard view.</returns>
    public async Task<IActionResult> Index()
    {
        DomainUser? currentUser = await GetCurrentDomainUserAsync();

        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Your account is not connected to an app user.";
            return RedirectToAction("Profile", "Account");
        }

        bool isAdmin = User.IsInRole(AppRoles.Admin);
        bool isCoach = User.IsInRole(AppRoles.Coach) || isAdmin;

        List<Group> visibleGroups = isAdmin
            ? _groupManager.GetAllGroups()
            : _groupManager.GetGroupsForUser(currentUser.Id);

        List<PracticeSession> upcomingSessions = isAdmin
            ? _practiceManager.GetUpcomingSessions()
            : _practiceManager.GetUpcomingSessionsForUser(currentUser.Id);

        List<DashboardPracticeCardViewModel> upcomingCards = upcomingSessions
            .Take(5)
            .Select(session => BuildPracticeCard(session, currentUser, isAdmin))
            .ToList();

        DashboardPracticeCardViewModel? nextPractice = upcomingCards.FirstOrDefault();

        if (nextPractice != null)
        {
            nextPractice.IsNextPractice = true;
        }

        List<DashboardPracticeCardViewModel> coachCards = new();

        if (isCoach)
        {
            List<Group> coachedGroups = isAdmin
                ? _groupManager.GetAllGroups()
                : _groupManager.GetGroupsCoachedByUser(currentUser.Id);

            List<int> coachedGroupIds = coachedGroups
                .Select(group => group.Id)
                .ToList();

            coachCards = _practiceManager
                .GetUpcomingSessionsForGroups(coachedGroupIds)
                .Take(6)
                .Select(session => BuildPracticeCard(session, currentUser, isAdmin))
                .ToList();
        }

        DashboardViewModel viewModel = new DashboardViewModel
        {
            CurrentUser = currentUser,
            IsAdmin = isAdmin,
            IsCoach = isCoach,
            GroupCount = visibleGroups.Count,
            UpcomingPracticeCount = upcomingSessions.Count,
            MissingResponseCount = coachCards.Sum(card => card.Summary?.NoResponseCount ?? 0),
            NextPractice = nextPractice,
            UpcomingPractices = upcomingCards,
            CoachPractices = coachCards
        };

        return View(viewModel);
    }

    /// <summary>
    /// Shows the public landing page for visitors.
    /// </summary>
    /// <returns>The landing page or the dashboard.</returns>
    [AllowAnonymous]
    public IActionResult Landing()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    /// <summary>
    /// Shows the privacy page.
    /// </summary>
    /// <returns>The privacy view.</returns>
    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Shows the error page.
    /// </summary>
    /// <returns>The error view.</returns>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    /// <summary>
    /// Builds one practice card for the dashboard.
    /// </summary>
    /// <param name="session">The practice session to show.</param>
    /// <param name="currentUser">The current logged-in domain user.</param>
    /// <param name="isAdmin">Whether the current user is an admin.</param>
    /// <returns>The dashboard practice card data.</returns>
    private DashboardPracticeCardViewModel BuildPracticeCard(
        PracticeSession session,
        DomainUser currentUser,
        bool isAdmin)
    {
        Group group = _groupManager.FindGroupById(session.GroupId)
            ?? new Group
            {
                Id = session.GroupId,
                Name = "Unknown group"
            };

        Attendance? currentUserAttendance =
            _attendanceManager.GetAttendanceForUserAndSession(
                currentUser.Id,
                session.Id);

        AttendanceSummary summary = _attendanceManager.GetSummary(session.Id);

        bool canManagePractice = isAdmin ||
                                 _groupManager.IsUserCoachOfGroup(group.Id, currentUser.Id);

        return new DashboardPracticeCardViewModel
        {
            PracticeSession = session,
            Group = group,
            CurrentUserAttendance = currentUserAttendance,
            Summary = summary,
            CanManagePractice = canManagePractice
        };
    }

    /// <summary>
    /// Gets the current logged-in domain user.
    /// </summary>
    /// <returns>The matching domain user, or null.</returns>
    private async Task<DomainUser?> GetCurrentDomainUserAsync()
    {
        string? identityUserId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            return null;
        }

        return await Task.FromResult(_groupManager.FindUserByIdentityUserId(identityUserId));
    }
}
