using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;

namespace SquadCheck.Controllers;

/// <summary>
/// Controls the pages for practice sessions.
/// </summary>
public class PracticesController : Controller
{
    private readonly PracticeManager _practiceManager;
    private readonly GroupManager _groupManager;
    private readonly AttendanceManager _attendanceManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Gets the managers this controller needs.
    /// </summary>
    /// <param name="practiceManager">Manager used for practice sessions.</param>
    /// <param name="groupManager">Manager used to read groups and members.</param>
    /// <param name="attendanceManager">Manager used to read attendance data.</param>
    /// <param name="userManager">Manager used to read the current Identity user.</param>
    public PracticesController(
        PracticeManager practiceManager,
        GroupManager groupManager,
        AttendanceManager attendanceManager,
        UserManager<ApplicationUser> userManager)
    {
        _practiceManager = practiceManager;
        _groupManager = groupManager;
        _attendanceManager = attendanceManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Shows the list of practices.
    /// </summary>
    /// <returns>The practice index view.</returns>
    public IActionResult Index()
    {
        List<PracticeSession> sessions = _practiceManager.GetAllSessions();

        return View(sessions);
    }

    /// <summary>
    /// Shows one practice with attendance information.
    /// </summary>
    /// <param name="id">The id of the practice session to show.</param>
    /// <returns>The practice details view, or a not found result.</returns>
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

        List<Attendance> attendances = _attendanceManager.GetAttendanceForSession(session.Id);

        User? currentDomainUser = null;
        Attendance? currentUserAttendance = null;

        string? identityUserId = _userManager.GetUserId(User);

        if (!string.IsNullOrWhiteSpace(identityUserId))
        {
            currentDomainUser = _groupManager.FindUserByIdentityUserId(identityUserId);

            if (currentDomainUser != null)
            {
                currentUserAttendance = attendances.FirstOrDefault(attendance =>
                    attendance.UserId == currentDomainUser.Id);
            }
        }

        PracticeDetailsViewModel viewModel = new PracticeDetailsViewModel
        {
            PracticeSession = session,
            Group = group,
            Users = _groupManager.GetAllUsers(),
            Members = _groupManager.GetMembersForGroup(group.Id),
            Attendances = attendances,
            Summary = _attendanceManager.GetSummary(session.Id),
            MarkAttendance = new MarkAttendanceViewModel
            {
                PracticeSessionId = session.Id
            },
            CurrentUser = currentDomainUser,
            CurrentUserAttendance = currentUserAttendance
        };

        return View(viewModel);
    }

    [HttpGet]
    /// <summary>
    /// Shows the create practice form.
    /// </summary>
    /// <param name="groupId">An optional group id to preselect in the form.</param>
    /// <returns>The create practice view, a redirect, or a not found result.</returns>
    public IActionResult Create(int? groupId)
    {
        List<Group> groups = _groupManager.GetAllGroups();

        if (groups.Count == 0)
        {
            TempData["ErrorMessage"] = "You need to create a group before creating a practice.";
            return RedirectToAction("Index", "Groups");
        }

        CreatePracticeSessionViewModel viewModel = new CreatePracticeSessionViewModel();

        if (groupId != null)
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
    /// <summary>
    /// Saves a new practice from the form.
    /// </summary>
    /// <param name="viewModel">The posted practice form values.</param>
    /// <returns>A redirect to practice details, or the form with validation errors.</returns>
    public IActionResult Create(CreatePracticeSessionViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            PrepareGroupSelectList(viewModel.GroupId);
            return View(viewModel);
        }

        try
        {
            PracticeSession practiceSession = _practiceManager.CreateSession(
                viewModel.GroupId,
                viewModel.Title,
                viewModel.StartTime,
                viewModel.EndTime,
                viewModel.Location,
                viewModel.MaxParticipants);

            TempData["SuccessMessage"] = "Practice session was created successfully.";

            return RedirectToAction("Details", new { id = practiceSession.Id });
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
    /// <summary>
    /// Shows the edit practice form.
    /// </summary>
    /// <param name="id">The id of the practice session to edit.</param>
    /// <returns>The edit practice view, or a not found result.</returns>
    public IActionResult Edit(int id)
    {
        PracticeSession? practiceSession = _practiceManager.FindSessionById(id);

        if (practiceSession == null)
        {
            return NotFound();
        }

        EditPracticeSessionViewModel viewModel = new EditPracticeSessionViewModel
        {
            Id = practiceSession.Id,
            GroupId = practiceSession.GroupId,
            Title = practiceSession.Title,
            StartTime = practiceSession.StartTime,
            EndTime = practiceSession.EndTime,
            Location = practiceSession.Location,
            MaxParticipants = practiceSession.MaxParticipants
        };

        PrepareGroupSelectList(practiceSession.GroupId);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Saves changes to a practice.
    /// </summary>
    /// <param name="viewModel">The posted edit form values.</param>
    /// <returns>A redirect to practice details, or the form with validation errors.</returns>
    public IActionResult Edit(EditPracticeSessionViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            PrepareGroupSelectList(viewModel.GroupId);
            return View(viewModel);
        }

        try
        {
            PracticeSession practiceSession = _practiceManager.UpdateSession(
                viewModel.Id,
                viewModel.Title,
                viewModel.StartTime,
                viewModel.EndTime,
                viewModel.Location,
                viewModel.MaxParticipants);

            TempData["SuccessMessage"] = "Practice session was updated successfully.";

            return RedirectToAction("Details", new { id = practiceSession.Id });
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
    /// <summary>
    /// Cancels a practice.
    /// </summary>
    /// <param name="id">The id of the practice session to cancel.</param>
    /// <returns>A redirect to the group details page or practice index page.</returns>
    public IActionResult Cancel(int id)
    {
        PracticeSession? practiceSession = _practiceManager.FindSessionById(id);

        if (practiceSession == null)
        {
            TempData["ErrorMessage"] = "Practice session could not be found.";
            return RedirectToAction("Index");
        }

        int groupId = practiceSession.GroupId;

        bool cancelled = _practiceManager.CancelSession(id);

        if (cancelled)
        {
            TempData["SuccessMessage"] = "Practice session was cancelled.";
            return RedirectToAction("Details", "Groups", new { id = groupId });
        }

        TempData["ErrorMessage"] = "Practice session could not be cancelled.";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Prepares the group dropdown used by the create and edit forms.
    /// </summary>
    /// <param name="selectedGroupId">The group id that should be selected in the dropdown.</param>
    private void PrepareGroupSelectList(int? selectedGroupId = null)
    {
        List<Group> groups = _groupManager.GetAllGroups();

        ViewBag.Groups = new SelectList(groups, "Id", "Name", selectedGroupId);
    }
}
