using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;

namespace SquadCheck.Controllers;

public class PracticesController : Controller
{
    private readonly PracticeManager _practiceManager;
    private readonly GroupManager _groupManager;
    private readonly AttendanceManager _attendanceManager;

    public PracticesController(
        PracticeManager practiceManager,
        GroupManager groupManager,
        AttendanceManager attendanceManager)
    {
        _practiceManager = practiceManager;
        _groupManager = groupManager;
        _attendanceManager = attendanceManager;
    }

    public IActionResult Index()
    {
        List<PracticeSession> sessions = _practiceManager.GetAllSessions();

        return View(sessions);
    }

    public IActionResult Details(int id)
    {
        PracticeSession? session = _practiceManager.FindSessionById(id);

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

    [HttpGet]
    public IActionResult Create(int? groupId)
    {
        List<Group> groups = _groupManager.GetAllGroups();

        if (groups.Count == 0)
        {
            TempData["ErrorMessage"] = "You need to create a group before creating a practice.";
            return RedirectToAction("Index", "Groups");
        }

        CreatePracticeSessionViewModel viewModel = new CreatePracticeSessionViewModel();

        if (groupId.HasValue)
        {
            Group? group = _groupManager.FindGroupById(groupId.Value);

            if (group == null)
            {
                return NotFound();
            }

            viewModel.GroupId = group.Id;
        }

        PrepareGroupSelectList(viewModel.GroupId);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreatePracticeSessionViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            PrepareGroupSelectList(viewModel.GroupId);
            return View(viewModel);
        }

        try
        {
            PracticeSession session = _practiceManager.CreateSession(
                viewModel.GroupId,
                viewModel.Title,
                viewModel.StartTime,
                viewModel.EndTime,
                viewModel.Location,
                viewModel.MaxParticipants);

            TempData["SuccessMessage"] = "Practice session was created successfully.";

            return RedirectToAction(nameof(Details), new { id = session.Id });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            PrepareGroupSelectList(viewModel.GroupId);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            PrepareGroupSelectList(viewModel.GroupId);
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        PracticeSession? session = _practiceManager.FindSessionById(id);

        if (session == null)
        {
            return NotFound();
        }

        EditPracticeSessionViewModel viewModel = new EditPracticeSessionViewModel
        {
            Id = session.Id,
            GroupId = session.GroupId,
            Title = session.Title,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Location = session.Location,
            MaxParticipants = session.MaxParticipants
        };

        PrepareGroupSelectList(session.GroupId);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditPracticeSessionViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            PrepareGroupSelectList(viewModel.GroupId);
            return View(viewModel);
        }

        try
        {
            PracticeSession session = _practiceManager.UpdateSession(
                viewModel.Id,
                viewModel.Title,
                viewModel.StartTime,
                viewModel.EndTime,
                viewModel.Location,
                viewModel.MaxParticipants);

            TempData["SuccessMessage"] = "Practice session was updated successfully.";

            return RedirectToAction(nameof(Details), new { id = session.Id });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            PrepareGroupSelectList(viewModel.GroupId);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            PrepareGroupSelectList(viewModel.GroupId);
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        PracticeSession? session = _practiceManager.FindSessionById(id);

        if (session == null)
        {
            TempData["ErrorMessage"] = "Practice session could not be found.";
            return RedirectToAction(nameof(Index));
        }

        int groupId = session.GroupId;

        bool cancelled = _practiceManager.CancelSession(id);

        if (cancelled)
        {
            TempData["SuccessMessage"] = "Practice session was cancelled.";
            return RedirectToAction("Details", "Groups", new { id = groupId });
        }

        TempData["ErrorMessage"] = "Practice session could not be cancelled.";
        return RedirectToAction(nameof(Index));
    }

    private void PrepareGroupSelectList(int? selectedGroupId = null)
    {
        List<Group> groups = _groupManager.GetAllGroups();

        ViewBag.Groups = new SelectList(groups, "Id", "Name", selectedGroupId);
    }
}
