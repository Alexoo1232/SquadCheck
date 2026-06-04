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
        PracticeSession? session = _practiceManager.FindSessionById(viewModel.PracticeSessionId);

        if (session == null)
        {
            TempData["ErrorMessage"] = "Practice session could not be found.";
            return RedirectToAction("Index", "Practices");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please choose a member before marking attendance.";
            return RedirectToAction("Details", "Practices", new { id = viewModel.PracticeSessionId });
        }

        try
        {
            _attendanceManager.MarkAttendance(
                viewModel.PracticeSessionId,
                viewModel.UserId,
                viewModel.Status,
                viewModel.Comment);

            TempData["SuccessMessage"] = "Attendance was updated successfully.";

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

    public IActionResult Summary(int practiceSessionId)
    {
        PracticeSession? session = _practiceManager.FindSessionById(practiceSessionId);

        if (session == null)
        {
            return NotFound();
        }

        Group? group = _groupManager.FindGroupById(session.GroupId);

        if (group == null)
        {
            return NotFound();
        }

        PracticeDetailsViewModel viewModel = new PracticeDetailsViewModel
        {
            PracticeSession = session,
            Group = group,
            Users = _groupManager.GetAllUsers(),
            Members = _groupManager.GetMembersForGroup(group.Id),
            Attendances = _attendanceManager.GetAttendanceForSession(session.Id),
            Summary = _attendanceManager.GetSummary(session.Id),
            MarkAttendance = new MarkAttendanceViewModel
            {
                PracticeSessionId = session.Id
            }
        };

        return View(viewModel);
    }
}
