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
/// Controls the pages for groups and group members.
/// </summary>
[Authorize]
public class GroupsController : Controller
{
    private readonly GroupManager _groupManager;
    private readonly PracticeManager _practiceManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Gets the managers this controller needs.
    /// </summary>
    /// <param name="groupManager">Manager used for groups, users, and memberships.</param>
    /// <param name="practiceManager">Manager used to read practice sessions for groups.</param>
    /// <param name="userManager">Manager used to read the current Identity user.</param>
    public GroupsController(
        GroupManager groupManager,
        PracticeManager practiceManager,
        UserManager<ApplicationUser> userManager)
    {
        _groupManager = groupManager;
        _practiceManager = practiceManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Shows the list of groups the current user can see.
    /// </summary>
    /// <returns>The group index view.</returns>
    public async Task<IActionResult> Index()
    {
        List<Group> groups;

        if (User.IsInRole(AppRoles.Admin))
        {
            groups = _groupManager.GetAllGroups();
        }
        else
        {
            DomainUser? currentUser = await GetCurrentDomainUserAsync();

            if (currentUser == null)
            {
                return RedirectToAction("Profile", "Account");
            }

            groups = _groupManager.GetGroupsForUser(currentUser.Id);
        }

        List<PracticeSession> sessions = _practiceManager.GetAllSessions();
        List<GroupListItemViewModel> viewModel = new();

        foreach (Group group in groups)
        {
            int practiceCount = 0;
            int upcomingPracticeCount = 0;

            foreach (PracticeSession session in sessions)
            {
                if (session.GroupId == group.Id)
                {
                    practiceCount++;

                    if (session.IsUpcoming())
                    {
                        upcomingPracticeCount++;
                    }
                }
            }

            bool canManageGroup = await CanManageGroupAsync(group.Id);

            viewModel.Add(new GroupListItemViewModel
            {
                Group = group,
                MemberCount = group.Members.Count,
                PracticeCount = practiceCount,
                UpcomingPracticeCount = upcomingPracticeCount,
                CanManageGroup = canManageGroup
            });
        }

        return View(viewModel);
    }

    /// <summary>
    /// Shows one group with its members and practices.
    /// </summary>
    /// <param name="id">The id of the group to show.</param>
    /// <returns>The group details view, or an access result.</returns>
    public async Task<IActionResult> Details(int id)
    {
        bool canView = await CanViewGroupAsync(id);

        if (!canView)
        {
            return Forbid();
        }

        Group? group = _groupManager.EnsureJoinCodeForGroup(id);

        if (group == null)
        {
            return NotFound();
        }

        bool canManage = await CanManageGroupAsync(id);
        string inviteLink = string.Empty;

        if (!string.IsNullOrWhiteSpace(group.JoinCode))
        {
            inviteLink = Url.Action(
                action: nameof(Join),
                controller: "Groups",
                values: new { code = group.JoinCode },
                protocol: Request.Scheme) ?? string.Empty;
        }

        GroupDetailsViewModel viewModel = new GroupDetailsViewModel
        {
            Group = group,
            Users = _groupManager.GetAllUsers(),
            Members = _groupManager.GetMembersForGroup(id),
            PracticeSessions = _practiceManager.GetSessionsForGroup(id),
            CanManageGroup = canManage,
            InviteLink = inviteLink
        };

        return View(viewModel);
    }

    /// <summary>
    /// Shows the join-group form.
    /// </summary>
    /// <param name="code">An optional join code to prefill.</param>
    /// <returns>The join-group view.</returns>
    [HttpGet]
    public IActionResult Join(string? code = null)
    {
        JoinGroupViewModel viewModel = new JoinGroupViewModel
        {
            Code = code ?? string.Empty
        };

        return View(viewModel);
    }

    /// <summary>
    /// Lets the current user join a group with a join code.
    /// </summary>
    /// <param name="viewModel">The posted join code.</param>
    /// <returns>A redirect to the joined group or the form again.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(JoinGroupViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        DomainUser? currentUser = await GetCurrentDomainUserAsync();

        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Your account is not connected to an app user.";
            return RedirectToAction("Profile", "Account");
        }

        try
        {
            Group group = _groupManager.JoinGroupByCode(
                currentUser.Id,
                viewModel.Code);

            TempData["SuccessMessage"] = $"You joined {group.Name}.";

            return RedirectToAction(nameof(Details), new { id = group.Id });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Shows the create group form.
    /// </summary>
    /// <returns>The create group view.</returns>
    [Authorize(Roles = AppRoles.AdminOrCoach)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateGroupViewModel());
    }

    /// <summary>
    /// Saves a new group from the form.
    /// </summary>
    /// <param name="viewModel">The posted group form values.</param>
    /// <returns>A redirect to the group details page, or the form with validation errors.</returns>
    [Authorize(Roles = AppRoles.AdminOrCoach)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGroupViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        DomainUser? currentUser = await GetCurrentDomainUserAsync();

        if (currentUser == null)
        {
            return Forbid();
        }

        try
        {
            Group group = _groupManager.CreateGroup(
                viewModel.Name,
                viewModel.Description);

            _groupManager.AddMemberToGroup(
                group.Id,
                currentUser.Id,
                GroupRole.Coach);

            TempData["SuccessMessage"] = "Group was created successfully.";

            return RedirectToAction(nameof(Details), new { id = group.Id });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Shows the add member form.
    /// </summary>
    /// <param name="groupId">The id of the group that will receive the member.</param>
    /// <returns>The add member view, or an access result.</returns>
    [HttpGet]
    public async Task<IActionResult> AddMember(int groupId)
    {
        bool canManage = await CanManageGroupAsync(groupId);

        if (!canManage)
        {
            return Forbid();
        }

        Group? group = _groupManager.FindGroupById(groupId);

        if (group == null)
        {
            return NotFound();
        }

        AddMemberViewModel viewModel = new AddMemberViewModel
        {
            GroupId = groupId,
            Role = GroupRole.Member
        };

        ViewBag.GroupName = group.Name;

        return View(viewModel);
    }

    /// <summary>
    /// Adds a member to a group.
    /// </summary>
    /// <param name="viewModel">The posted member form values.</param>
    /// <returns>A redirect to the group details page, or the form with validation errors.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(AddMemberViewModel viewModel)
    {
        bool canManage = await CanManageGroupAsync(viewModel.GroupId);

        if (!canManage)
        {
            return Forbid();
        }

        Group? group = _groupManager.FindGroupById(viewModel.GroupId);

        if (group == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.GroupName = group.Name;
            return View(viewModel);
        }

        try
        {
            string email = viewModel.Email.Trim();
            User? user = _groupManager
                .GetAllUsers()
                .FirstOrDefault(existingUser =>
                    string.Equals(existingUser.Email, email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                user = _groupManager.CreateUser(
                    viewModel.FullName,
                    viewModel.Email,
                    viewModel.Role == GroupRole.Coach);
            }

            _groupManager.AddMemberToGroup(
                viewModel.GroupId,
                user.Id,
                viewModel.Role);

            if (viewModel.Role == GroupRole.Coach)
            {
                await PromoteIdentityUserToCoachIfConnectedAsync(user);
            }

            TempData["SuccessMessage"] = "Member was added successfully.";

            return RedirectToAction(nameof(Details), new { id = viewModel.GroupId });
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewBag.GroupName = group.Name;
            return View(viewModel);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewBag.GroupName = group.Name;
            return View(viewModel);
        }
    }

    /// <summary>
    /// Removes a member from the group.
    /// </summary>
    /// <param name="groupId">The id of the group.</param>
    /// <param name="userId">The id of the user to remove.</param>
    /// <returns>A redirect back to the group details page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int groupId, int userId)
    {
        bool canManage = await CanManageGroupAsync(groupId);

        if (!canManage)
        {
            return Forbid();
        }

        bool removed = _groupManager.RemoveMemberFromGroup(groupId, userId);

        TempData[removed ? "SuccessMessage" : "ErrorMessage"] =
            removed ? "Member was removed successfully." : "Member could not be removed.";

        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    /// <summary>
    /// Generates a new join code for a group.
    /// </summary>
    /// <param name="id">The id of the group.</param>
    /// <returns>A redirect back to the group details page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegenerateJoinCode(int id)
    {
        bool canManage = await CanManageGroupAsync(id);

        if (!canManage)
        {
            return Forbid();
        }

        try
        {
            _groupManager.RegenerateJoinCode(id);
            TempData["SuccessMessage"] = "A new join code was generated.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Disables a group's join code.
    /// </summary>
    /// <param name="id">The id of the group.</param>
    /// <returns>A redirect back to the group details page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableJoinCode(int id)
    {
        bool canManage = await CanManageGroupAsync(id);

        if (!canManage)
        {
            return Forbid();
        }

        try
        {
            _groupManager.DisableJoinCode(id);
            TempData["SuccessMessage"] = "Join code was disabled.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Enables a group's join code.
    /// </summary>
    /// <param name="id">The id of the group.</param>
    /// <returns>A redirect back to the group details page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableJoinCode(int id)
    {
        bool canManage = await CanManageGroupAsync(id);

        if (!canManage)
        {
            return Forbid();
        }

        try
        {
            _groupManager.EnableJoinCode(id);
            TempData["SuccessMessage"] = "Join code was enabled.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Deletes a group.
    /// </summary>
    /// <param name="id">The id of the group to delete.</param>
    /// <returns>A redirect to the group index page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        bool canManage = await CanManageGroupAsync(id);

        if (!canManage)
        {
            return Forbid();
        }

        bool deleted = _groupManager.DeleteGroup(id);

        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] =
            deleted ? "Group was deleted successfully." : "Group could not be found.";

        return RedirectToAction(nameof(Index));
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
    /// Checks whether the current user can view a group.
    /// </summary>
    /// <param name="groupId">The id of the group.</param>
    /// <returns>True when the group may be viewed.</returns>
    private async Task<bool> CanViewGroupAsync(int groupId)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        DomainUser? currentUser = await GetCurrentDomainUserAsync();

        return currentUser != null &&
               _groupManager.IsUserMemberOfGroup(groupId, currentUser.Id);
    }

    /// <summary>
    /// Checks whether the current user can manage a group.
    /// </summary>
    /// <param name="groupId">The id of the group.</param>
    /// <returns>True when the group may be managed.</returns>
    private async Task<bool> CanManageGroupAsync(int groupId)
    {
        if (User.IsInRole(AppRoles.Admin))
        {
            return true;
        }

        DomainUser? currentUser = await GetCurrentDomainUserAsync();

        return currentUser != null &&
               _groupManager.IsUserCoachOfGroup(groupId, currentUser.Id);
    }

    /// <summary>
    /// Promotes a connected identity account to the coach role when needed.
    /// </summary>
    /// <param name="domainUser">The domain user that was made a coach.</param>
    private async Task PromoteIdentityUserToCoachIfConnectedAsync(DomainUser domainUser)
    {
        if (string.IsNullOrWhiteSpace(domainUser.IdentityUserId))
        {
            return;
        }

        ApplicationUser? identityUser =
            await _userManager.FindByIdAsync(domainUser.IdentityUserId);

        if (identityUser == null)
        {
            return;
        }

        bool isAdmin = await _userManager.IsInRoleAsync(identityUser, AppRoles.Admin);
        bool isCoach = await _userManager.IsInRoleAsync(identityUser, AppRoles.Coach);

        if (!isAdmin && !isCoach)
        {
            await _userManager.AddToRoleAsync(identityUser, AppRoles.Coach);
        }

        _groupManager.SetUserCoachStatus(domainUser.Id, true);
    }
}
