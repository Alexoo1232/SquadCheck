using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;

namespace SquadCheck.Controllers;

public class GroupsController : Controller
{
    private readonly GroupManager _groupManager;
    private readonly PracticeManager _practiceManager;

    public GroupsController(GroupManager groupManager, PracticeManager practiceManager)
    {
        _groupManager = groupManager;
        _practiceManager = practiceManager;
    }

    public IActionResult Index()
    {
        List<Group> groups = _groupManager.GetAllGroups();

        return View(groups);
    }

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
    public IActionResult Create()
    {
        return View(new CreateGroupViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

    [HttpGet]
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
            User? user = _groupManager
                .GetAllUsers()
                .FirstOrDefault(existingUser =>
                    existingUser.Email.Equals(
                        viewModel.Email.Trim(),
                        StringComparison.OrdinalIgnoreCase));

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

    [HttpPost]
    [ValidateAntiForgeryToken]
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

        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

        return RedirectToAction(nameof(Index));
    }
}
