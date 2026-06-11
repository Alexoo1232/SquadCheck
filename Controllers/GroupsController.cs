using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;

namespace SquadCheck.Controllers;

/// <summary>
/// Controls the pages for groups and group members.
/// </summary>
public class GroupsController : Controller
{
    private readonly GroupManager _groupManager;
    private readonly PracticeManager _practiceManager;

    /// <summary>
    /// Gets the managers this controller needs.
    /// </summary>
    /// <param name="groupManager">Manager used for groups, users, and memberships.</param>
    /// <param name="practiceManager">Manager used to read practice sessions for groups.</param>
    public GroupsController(GroupManager groupManager, PracticeManager practiceManager)
    {
        _groupManager = groupManager;
        _practiceManager = practiceManager;
    }

    /// <summary>
    /// Shows the list of groups.
    /// </summary>
    /// <returns>The group index view.</returns>
    public IActionResult Index()
    {
        List<Group> groups = _groupManager.GetAllGroups();
        List<PracticeSession> sessions = _practiceManager.GetAllSessions();
        List<GroupListItemViewModel> viewModel = new List<GroupListItemViewModel>();

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

            GroupListItemViewModel item = new GroupListItemViewModel
            {
                Group = group,
                MemberCount = group.Members.Count,
                PracticeCount = practiceCount,
                UpcomingPracticeCount = upcomingPracticeCount
            };

            viewModel.Add(item);
        }

        return View(viewModel);
    }

    /// <summary>
    /// Shows one group with its members and practices.
    /// </summary>
    /// <param name="id">The id of the group to show.</param>
    /// <returns>The group details view, or a not found result.</returns>
    public IActionResult Details(int id)
    {
        Group? group = _groupManager.FindGroupById(id);

        if (group == null)
        {
            return NotFound();
        }

        GroupDetailsViewModel viewModel = new GroupDetailsViewModel
        {
            Group = group,
            Users = _groupManager.GetAllUsers(),
            Members = _groupManager.GetMembersForGroup(id),
            PracticeSessions = _practiceManager.GetSessionsForGroup(id)
        };

        return View(viewModel);
    }

    [HttpGet]
    /// <summary>
    /// Shows the create group form.
    /// </summary>
    /// <returns>The create group view.</returns>
    public IActionResult Create()
    {
        return View(new CreateGroupViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Saves a new group from the form.
    /// </summary>
    /// <param name="viewModel">The posted group form values.</param>
    /// <returns>A redirect to the group details page, or the form with validation errors.</returns>
    public IActionResult Create(CreateGroupViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            Group group = _groupManager.CreateGroup(
                viewModel.Name,
                viewModel.Description);

            TempData["SuccessMessage"] = "Group was created successfully.";

            return RedirectToAction("Details", new { id = group.Id });
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

    [HttpGet]
    /// <summary>
    /// Shows the add member form.
    /// </summary>
    /// <param name="groupId">The id of the group that will receive the member.</param>
    /// <returns>The add member view, or a not found result.</returns>
    public IActionResult AddMember(int groupId)
    {
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Adds a member to a group.
    /// </summary>
    /// <param name="viewModel">The posted member form values.</param>
    /// <returns>A redirect to the group details page, or the form with validation errors.</returns>
    public IActionResult AddMember(AddMemberViewModel viewModel)
    {
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
            User? user = null;
            List<User> users = _groupManager.GetAllUsers();

            foreach (User existingUser in users)
            {
                bool sameEmail = existingUser.Email.Equals(
                    email,
                    StringComparison.OrdinalIgnoreCase);

                if (sameEmail)
                {
                    user = existingUser;
                    break;
                }
            }

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

            TempData["SuccessMessage"] = "Member was added successfully.";

            return RedirectToAction("Details", new { id = viewModel.GroupId });
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Removes a member from the group.
    /// </summary>
    /// <param name="groupId">The id of the group.</param>
    /// <param name="userId">The id of the user to remove.</param>
    /// <returns>A redirect back to the group details page.</returns>
    public IActionResult RemoveMember(int groupId, int userId)
    {
        Group? group = _groupManager.FindGroupById(groupId);

        if (group == null)
        {
            return NotFound();
        }

        bool removed = _groupManager.RemoveMemberFromGroup(groupId, userId);

        if (removed)
        {
            TempData["SuccessMessage"] = "Member was removed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Member could not be removed.";
        }

        return RedirectToAction("Details", new { id = groupId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Deletes a group.
    /// </summary>
    /// <param name="id">The id of the group to delete.</param>
    /// <returns>A redirect to the group index page.</returns>
    public IActionResult Delete(int id)
    {
        bool deleted = _groupManager.DeleteGroup(id);

        if (deleted)
        {
            TempData["SuccessMessage"] = "Group was deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Group could not be found.";
        }

        return RedirectToAction("Index");
    }
}
