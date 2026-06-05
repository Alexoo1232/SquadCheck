using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;

namespace SquadCheck.Controllers;

public class AttendanceController : Controller
{
    private readonly AttendanceManager _attendanceManager;
    private readonly PracticeManager _practiceManager;
    private readonly GroupManager _groupManager;

    public AttendanceController(
        AttendanceManager attendanceManager,
        PracticeManager practiceManager,
        GroupManager groupManager)
    {
        _attendanceManager = attendanceManager;
        _practiceManager = practiceManager;
        _groupManager = groupManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Mark(MarkAttendanceViewModel viewModel)
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
            TempData["ErrorMessage"] = "Please choose a member before marking attendance.";
            return RedirectToAction("Details", "Practices", new { id = practiceSessionId });
        }

        try
        {
            _attendanceManager.MarkAttendance(
                practiceSessionId,
                viewModel.UserId,
                viewModel.Status,
                viewModel.Comment);

            TempData["SuccessMessage"] = "Attendance was updated successfully.";

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
