using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;

namespace SquadCheck.Controllers;

/// <summary>
/// Controls the attendance pages and attendance form posts.
/// </summary>
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

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Marks attendance for the logged-in user.
    /// </summary>
    /// <param name="viewModel">The posted attendance form values.</param>
    /// <returns>A redirect back to the practice details page.</returns>
    public IActionResult Mark(MyAttendanceViewModel viewModel)
    {
        int practiceSessionId = viewModel.PracticeSessionId;
        PracticeSession? practiceSession = _practiceManager.FindSessionById(practiceSessionId);

        if (practiceSession == null)
        {
            TempData["ErrorMessage"] = "Practice session could not be found.";
            return RedirectToAction("Index", "Practices");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Attendance could not be updated.";
            return RedirectToAction("Details", "Practices", new { id = practiceSessionId });
        }

        string? identityUserId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(identityUserId))
        {
            TempData["ErrorMessage"] = "You need to log in before marking attendance.";
            return RedirectToAction("Login", "Account");
        }

        User? domainUser = _groupManager.FindUserByIdentityUserId(identityUserId);

        if (domainUser == null)
        {
            TempData["ErrorMessage"] = "Your account is not connected to an app user.";
            return RedirectToAction("Details", "Practices", new { id = practiceSessionId });
        }

        try
        {
            _attendanceManager.MarkAttendance(
                practiceSessionId,
                domainUser.Id,
                viewModel.Status,
                viewModel.Comment);

            TempData["SuccessMessage"] = "Your attendance was updated.";

            return RedirectToAction("Details", "Practices", new { id = practiceSessionId });
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Details", "Practices", new { id = practiceSessionId });
        }
        catch (ArgumentException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction("Details", "Practices", new { id = practiceSessionId });
        }
    }

    /// <summary>
    /// Shows the attendance summary for one practice.
    /// </summary>
    /// <param name="practiceSessionId">The id of the practice session to summarize.</param>
    /// <returns>The attendance summary view, or a not found result.</returns>
    public IActionResult Summary(int practiceSessionId)
    {
        PracticeSession? practiceSession = _practiceManager.FindSessionById(practiceSessionId);

        if (practiceSession == null)
        {
            return NotFound();
        }

        Group? group = _groupManager.FindGroupById(practiceSession.GroupId);

        if (group == null)
        {
            return NotFound();
        }

        PracticeDetailsViewModel viewModel = new PracticeDetailsViewModel
        {
            PracticeSession = practiceSession,
            Group = group,
            Users = _groupManager.GetAllUsers(),
            Members = _groupManager.GetMembersForGroup(group.Id),
            Attendances = _attendanceManager.GetAttendanceForSession(practiceSession.Id),
            Summary = _attendanceManager.GetSummary(practiceSession.Id),
            MarkAttendance = new MarkAttendanceViewModel
            {
                PracticeSessionId = practiceSession.Id
            }
        };

        return View(viewModel);
    }
}
