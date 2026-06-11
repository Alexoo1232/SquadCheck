using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;
using SquadCheck.Security;

using DomainUser = SquadCheck.Models.User;

namespace SquadCheck.Controllers;

/// <summary>
/// Controls the pages for practice sessions.
/// </summary>
[Authorize]
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
    /// Shows the list of practices the current user can see.
    /// </summary>
    /// <returns>The practice index view.</returns>
    public async Task<IActionResult> Index()
    {
        List<PracticeSession> sessions;

        if (User.IsInRole(AppRoles.Admin))
        {
            sessions = _practiceManager.GetAllSessions();
        }
        else
        {
            DomainUser? currentUser = await GetCurrentDomainUserAsync();

            if (currentUser == null)
            {
                return RedirectToAction("Profile", "Account");
            }

            sessions = _practiceManager.GetSessionsForUser(currentUser.Id);
        }

        return View(sessions);
    }

    /// <summary>
    /// Shows one practice with attendance information.
    /// </summary>
    /// <param name="id">The id of the practice session to show.</param>
    /// <returns>The practice details view, or an access result.</returns>
    public async Task<IActionResult> Details(int id)
    {
        bool canView = await CanViewSessionAsync(id);

        if (!canView)
        {
            return Forbid();
        }

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
        DomainUser? currentDomainUser = await GetCurrentDomainUserAsync();
        Attendance? currentUserAttendance = null;

        if (currentDomainUser != null)
        {
            currentUserAttendance = attendances.FirstOrDefault(attendance =>
                attendance.UserId == currentDomainUser.Id);
        }

        PracticeDetailsViewModel viewModel = new PracticeDetailsViewModel
        {
            PracticeSession = session,
            Group = group,
            Users = _groupManager.GetAllUsers(),
            Members = _groupManager.GetMembersForGroup(group.Id),
            Attendances = attendances,
            Summary = _attendanceManager.GetSummary(session.Id),
            CurrentUser = currentDomainUser,
            CurrentUserAttendance = currentUserAttendance,
            CanManagePractice = await CanManageSessionAsync(id)
        };

        return View(viewModel);
    }

    /// <summary>
    /// Shows the create practice form.
    /// </summary>
    /// <param name="groupId">An optional group id to preselect in the form.</param>
    /// <returns>The create practice view or a redirect.</returns>
    [Authorize(Roles = AppRoles.AdminOrCoach)]
    [HttpGet]
    public async Task<IActionResult> Create(int? groupId)
    {
        List<Group> manageableGroups = await GetManageableGroupsAsync();

        if (manageableGroups.Count == 0)
        {
            TempData["ErrorMessage"] = "You need to be a coach of a group before creating a practice.";
            return RedirectToAction("Index", "Groups");
        }

        CreatePracticeSessionViewModel viewModel = new();

        if (groupId.HasValue)
        {
            bool canManageGroup = manageableGroups.Any(group => group.Id == groupId.Value);

            if (!canManageGroup)
            {
                return Forbid();
            }

            viewModel.GroupId = groupId.Value;
        }

        PrepareGroupSelectList(manageableGroups, viewModel.GroupId);

        return View(viewModel);
    }

    /// <summary>
    /// Saves a new practice from the form.
    /// </summary>
    /// <param name="viewModel">The posted practice form values.</param>
    /// <returns>A redirect to practice details, or the form with validation errors.</returns>
    [Authorize(Roles = AppRoles.AdminOrCoach)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePracticeSessionViewModel viewModel)
    {
        List<Group> manageableGroups = await GetManageableGroupsAsync();
        bool canCreateForGroup = manageableGroups.Any(group => group.Id == viewModel.GroupId);

        if (!canCreateForGroup)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            PrepareGroupSelectList(manageableGroups, viewModel.GroupId);
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
            PrepareGroupSelectList(manageableGroups, viewModel.GroupId);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            PrepareGroupSelectList(manageableGroups, viewModel.GroupId);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Shows the edit practice form.
    /// </summary>
    /// <param name="id">The id of the practice session to edit.</param>
    /// <returns>The edit practice view, or an access result.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        bool canManage = await CanManageSessionAsync(id);

        if (!canManage)
        {
            return Forbid();
        }

        PracticeSession? session = _practiceManager.FindSessionById(id);

        if (session == null)
        {
            return NotFound();
        }

        EditPracticeSessionViewModel viewModel = new()
        {
            Id = session.Id,
            GroupId = session.GroupId,
            Title = session.Title,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Location = session.Location,
            MaxParticipants = session.MaxParticipants
        };

        PrepareGroupSelectList(await GetManageableGroupsAsync(), session.GroupId);

        return View(viewModel);
    }

    /// <summary>
    /// Saves changes to a practice.
    /// </summary>
    /// <param name="viewModel">The posted edit form values.</param>
    /// <returns>A redirect to practice details, or the form with validation errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditPracticeSessionViewModel viewModel)
    {
        bool canManage = await CanManageSessionAsync(viewModel.Id);

        if (!canManage)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            PrepareGroupSelectList(await GetManageableGroupsAsync(), viewModel.GroupId);
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
            PrepareGroupSelectList(await GetManageableGroupsAsync(), viewModel.GroupId);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            PrepareGroupSelectList(await GetManageableGroupsAsync(), viewModel.GroupId);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Cancels a practice.
    /// </summary>
    /// <param name="id">The id of the practice session to cancel.</param>
    /// <returns>A redirect to the related group or practice index page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        bool canManage = await CanManageSessionAsync(id);

        if (!canManage)
        {
            return Forbid();
        }

        PracticeSession? session = _practiceManager.FindSessionById(id);

        if (session == null)
        {
            TempData["ErrorMessage"] = "Practice session could not be found.";
            return RedirectToAction(nameof(Index));
        }

        int groupId = session.GroupId;
        bool cancelled = _practiceManager.CancelSession(id);

        TempData[cancelled ? "SuccessMessage" : "ErrorMessage"] =
            cancelled ? "Practice session was cancelled." : "Practice session could not be cancelled.";

        return RedirectToAction("Details", "Groups", new { id = groupId });
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

    /// <summary>
    /// Checks whether the current user can view a practice.
    /// </summary>
    /// <param name="sessionId">The id of the practice session.</param>
    /// <returns>True when the practice may be viewed.</returns>
    private async Task<bool> CanViewSessionAsync(int sessionId)
    {
        PracticeSession? session = _practiceManager.FindSessionById(sessionId);

        if (session == null)
        {
            return false;
        }

        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        DomainUser? currentUser = await GetCurrentDomainUserAsync();

        return currentUser != null &&
               _groupManager.IsUserMemberOfGroup(session.GroupId, currentUser.Id);
    }

    /// <summary>
    /// Checks whether the current user can manage a practice.
    /// </summary>
    /// <param name="sessionId">The id of the practice session.</param>
    /// <returns>True when the practice may be managed.</returns>
    private async Task<bool> CanManageSessionAsync(int sessionId)
    {
        PracticeSession? session = _practiceManager.FindSessionById(sessionId);

        if (session == null)
        {
            return false;
        }

        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        DomainUser? currentUser = await GetCurrentDomainUserAsync();

        return currentUser != null &&
               _groupManager.IsUserCoachOfGroup(session.GroupId, currentUser.Id);
    }

    /// <summary>
    /// Gets the groups the current user may manage practices for.
    /// </summary>
    /// <returns>The manageable groups.</returns>
    private async Task<List<Group>> GetManageableGroupsAsync()
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return _groupManager.GetAllGroups();
        }

        DomainUser? currentUser = await GetCurrentDomainUserAsync();

        if (currentUser == null)
        {
            return new List<Group>();
        }

        return _groupManager.GetGroupsCoachedByUser(currentUser.Id);
    }

    /// <summary>
    /// Prepares the group dropdown used by the create and edit forms.
    /// </summary>
    /// <param name="groups">The groups that may be selected.</param>
    /// <param name="selectedGroupId">The group id that should be selected in the dropdown.</param>
    private void PrepareGroupSelectList(
        List<Group> groups,
        int? selectedGroupId = null)
    {
        ViewBag.Groups = new SelectList(groups, "Id", "Name", selectedGroupId);
    }
}
