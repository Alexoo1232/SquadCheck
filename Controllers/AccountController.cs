using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Models.ViewModels.Account;
using SquadCheck.Security;

namespace SquadCheck.Controllers;

/// <summary>
/// Controls registration, login, logout, and profile pages.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly GroupManager _groupManager;

    /// <summary>
    /// Gets the managers this controller needs.
    /// </summary>
    /// <param name="userManager">The ASP.NET Identity user manager.</param>
    /// <param name="signInManager">The ASP.NET Identity sign-in manager.</param>
    /// <param name="groupManager">The domain group manager.</param>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        GroupManager groupManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _groupManager = groupManager;
    }

    /// <summary>
    /// Shows the register form.
    /// </summary>
    /// <param name="returnUrl">An optional return URL.</param>
    /// <returns>The register view.</returns>
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new RegisterViewModel());
    }

    /// <summary>
    /// Creates a new account and links it to a domain user.
    /// </summary>
    /// <param name="viewModel">The posted register form values.</param>
    /// <param name="returnUrl">An optional return URL.</param>
    /// <returns>The next page after registration, or the register view again.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel viewModel,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        ApplicationUser identityUser = new ApplicationUser
        {
            UserName = viewModel.Email.Trim(),
            Email = viewModel.Email.Trim(),
            FullName = viewModel.FullName.Trim()
        };

        IdentityResult result = await _userManager.CreateAsync(
            identityUser,
            viewModel.Password);

        if (result.Succeeded)
        {
            User domainUser = _groupManager.CreateOrConnectIdentityUser(
                viewModel.FullName,
                viewModel.Email,
                identityUser.Id);

            string roleToAssign = domainUser.IsCoach
                ? AppRoles.Coach
                : AppRoles.Member;

            await _userManager.AddToRoleAsync(identityUser, roleToAssign);

            await _signInManager.SignInAsync(identityUser, isPersistent: false);

            return RedirectSafely(returnUrl);
        }

        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(viewModel);
    }

    /// <summary>
    /// Shows the login form.
    /// </summary>
    /// <param name="returnUrl">An optional return URL.</param>
    /// <returns>The login view.</returns>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    /// <summary>
    /// Signs a user in.
    /// </summary>
    /// <param name="viewModel">The posted login form values.</param>
    /// <param name="returnUrl">An optional return URL.</param>
    /// <returns>The next page after login, or the login view again.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel viewModel,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        Microsoft.AspNetCore.Identity.SignInResult result =
            await _signInManager.PasswordSignInAsync(
                viewModel.Email.Trim(),
                viewModel.Password,
                viewModel.RememberMe,
                lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectSafely(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");

        return View(viewModel);
    }

    /// <summary>
    /// Signs the current user out.
    /// </summary>
    /// <returns>A redirect to the home page.</returns>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Shows the current user's profile page.
    /// </summary>
    /// <returns>The profile view, or the login page.</returns>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        ApplicationUser? identityUser = await _userManager.GetUserAsync(User);

        if (identityUser == null)
        {
            return RedirectToAction(nameof(Login));
        }

        User? domainUser = _groupManager.FindUserByIdentityUserId(identityUser.Id);

        ProfileViewModel viewModel = new ProfileViewModel
        {
            FullName = identityUser.FullName,
            Email = identityUser.Email ?? string.Empty,
            DomainUserId = domainUser?.Id,
            IsConnectedToDomainUser = domainUser != null
        };

        return View(viewModel);
    }

    /// <summary>
    /// Shows the access denied page.
    /// </summary>
    /// <returns>The access denied view.</returns>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Redirects to a local return URL when it is safe.
    /// </summary>
    /// <param name="returnUrl">The optional return URL.</param>
    /// <returns>A redirect result.</returns>
    private IActionResult RedirectSafely(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
