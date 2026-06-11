using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels;
using SquadCheck.Security;

namespace SquadCheck.Controllers;

/// <summary>
/// Controls admin-only role management pages.
/// </summary>
[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GroupManager _groupManager;

    /// <summary>
    /// Gets the services used by the admin pages.
    /// </summary>
    /// <param name="userManager">The ASP.NET Identity user manager.</param>
    /// <param name="groupManager">The domain group manager.</param>
    public AdminController(
        UserManager<ApplicationUser> userManager,
        GroupManager groupManager)
    {
        _userManager = userManager;
        _groupManager = groupManager;
    }

    /// <summary>
    /// Shows the list of registered users and their roles.
    /// </summary>
    /// <returns>The admin users view.</returns>
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        List<ApplicationUser> identityUsers = await _userManager.Users
            .OrderBy(user => user.Email)
            .ToListAsync();

        List<AdminUserViewModel> viewModel = new();

        foreach (ApplicationUser identityUser in identityUsers)
        {
            IList<string> roles = await _userManager.GetRolesAsync(identityUser);
            User? domainUser = _groupManager.FindUserByIdentityUserId(identityUser.Id);

            viewModel.Add(new AdminUserViewModel
            {
                IdentityUserId = identityUser.Id,
                FullName = identityUser.FullName,
                Email = identityUser.Email ?? string.Empty,
                DomainUserId = domainUser?.Id,
                IsDomainCoach = domainUser?.IsCoach ?? false,
                Roles = roles.ToList()
            });
        }

        return View(viewModel);
    }

    /// <summary>
    /// Updates one user's role.
    /// </summary>
    /// <param name="identityUserId">The ASP.NET Identity user id.</param>
    /// <param name="role">The role to assign.</param>
    /// <returns>A redirect back to the admin users page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string identityUserId, string role)
    {
        if (role != AppRoles.Admin &&
            role != AppRoles.Coach &&
            role != AppRoles.Member)
        {
            TempData["ErrorMessage"] = "Invalid role.";
            return RedirectToAction(nameof(Users));
        }

        ApplicationUser? identityUser = await _userManager.FindByIdAsync(identityUserId);

        if (identityUser == null)
        {
            TempData["ErrorMessage"] = "User could not be found.";
            return RedirectToAction(nameof(Users));
        }

        IList<string> currentRoles = await _userManager.GetRolesAsync(identityUser);

        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
        }

        IdentityResult result = await _userManager.AddToRoleAsync(identityUser, role);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Role could not be updated.";
            return RedirectToAction(nameof(Users));
        }

        User? domainUser = _groupManager.FindUserByIdentityUserId(identityUser.Id);

        if (domainUser != null)
        {
            bool shouldBeCoach = role == AppRoles.Admin || role == AppRoles.Coach;
            _groupManager.SetUserCoachStatus(domainUser.Id, shouldBeCoach);
        }

        TempData["SuccessMessage"] = "User role was updated.";

        return RedirectToAction(nameof(Users));
    }
}
