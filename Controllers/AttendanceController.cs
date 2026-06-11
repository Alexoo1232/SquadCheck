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
/// Controls the attendance pages and attendance form posts.
/// </summary>
[Authorize]
public class AttendanceController : Controller
{
    private readonly AttendanceManager _attendanceManager;
    private readonly PracticeManager _practiceManager;
    private readonly GroupManager _groupManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Gets the managers this controller needs.
    /// </summary>
    /// <param name="attendanceManager">Manager used to save and read attendance.</param>
    /// <param name="practiceManager">Manager used to find practice sessions.</param>
    /// <param name="groupManager">Manager used to find groups and members.</param>
    /// <param name="userManager">Manager used to read the current Identity user.</param>
    public AttendanceController(
        AttendanceManager attendanceManager,
        PracticeManager practiceManager,
        GroupManager groupManager,
        UserManager<ApplicationUser> userManager)
    {
        _attendanceManager = attendanceManager;
        _practiceManager = practiceManager;
        _groupManager = groupManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Marks attendance for the logged-in user.
    /// </summary>
    /// <param name="viewModel">The posted attendance form values.</param>
    /// <returns>A redirect back to the practice details page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Mark(MyAttendanceViewModel viewModel)
    {
        PracticeSession? session = _practiceManager.FindSessionById(viewModel.PracticeSessionId);

        if (session == null)
        {
            TempData["ErrorMessage"] = "Practice session could not be found.";
            return RedirectToAction("Index", "Practices");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Attendance could not be updated.";
            return RedirectToAction("Details", "Practices", new { id = viewModel.PracticeSessionId });
        }

        string? identityUserId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            TempData["ErrorMessage"] = "You need to log in before marking attendance.";
            return RedirectToAction("Login", "Account");
        }

        DomainUser? domainUser = _groupManager.FindUserByIdentityUserId(identityUserId);

        if (domainUser == null)
        {
            TempData["ErrorMessage"] = "Your account is not connected to an app user.";
            return RedirectToAction("Details", "Practices", new { id = viewModel.PracticeSessionId });
        }

        if (!_groupManager.IsUserMemberOfGroup(session.GroupId, domainUser.Id))
        {
            return Forbid();
        }

        try
        {
            _attendanceManager.MarkAttendance(
                viewModel.PracticeSessionId,
                domainUser.Id,
                viewModel.Status,
                viewModel.Comment);

            TempData["SuccessMessage"] = "Your attendance was updated.";

            return RedirectToAction("Details", "Practices", new { id = viewModel.PracticeSessionId });
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Details", "Practices", new { id = viewModel.PracticeSessionId });
        }
        catch (ArgumentException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Details", "Practices", new { id = viewModel.PracticeSessionId });
        }
    }

    /// <summary>
    /// Shows the attendance summary for one practice.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session to summarize.</param>
    /// <returns>The attendance summary view, or an access result.</returns>
    public async Task<IActionResult> Summary(int practiceSessionId)
    {
        PracticeSession? session = _practiceManager.FindSessionById(practiceSessionId);

        if (session == null)
        {
            return NotFound();
        }

        bool canViewSummary = await CanManageSessionAsync(session);

        if (!canViewSummary)
        {
            return Forbid();
        }

        Group? group = _groupManager.FindGroupById(session.GroupId);

        if (group == null)
        {
            return NotFound();
        }

        PracticeDetailsViewModel viewModel = new()
        {
            PracticeSession = session,
            Group = group,
            Users = _groupManager.GetAllUsers(),
            Members = _groupManager.GetMembersForGroup(group.Id),
            Attendances = _attendanceManager.GetAttendanceForSession(session.Id),
            Summary = _attendanceManager.GetSummary(session.Id),
            CanManagePractice = true
        };

        return View(viewModel);
    }

    /// <summary>
    /// Checks whether the current user may manage a practice session.
    /// </summary>
    /// <param name="session">The practice session to check.</param>
    /// <returns>True when the user may manage the session.</returns>
    private async Task<bool> CanManageSessionAsync(PracticeSession session)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        string? identityUserId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            return false;
        }

        DomainUser? domainUser = _groupManager.FindUserByIdentityUserId(identityUserId);

        return await Task.FromResult(
            domainUser != null &&
            _groupManager.IsUserCoachOfGroup(session.GroupId, domainUser.Id));
    }
}
