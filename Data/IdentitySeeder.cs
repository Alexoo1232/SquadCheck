using Microsoft.AspNetCore.Identity;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Security;

namespace SquadCheck.Data;

/// <summary>
/// Seeds Identity roles and an optional first admin user.
/// </summary>
public static class IdentitySeeder
{
    /// <summary>
    /// Creates missing roles and seeds the configured admin user.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        RoleManager<IdentityRole> roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        UserManager<ApplicationUser> userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        IConfiguration configuration =
            scope.ServiceProvider.GetRequiredService<IConfiguration>();

        GroupManager groupManager =
            scope.ServiceProvider.GetRequiredService<GroupManager>();

        await EnsureRoleExistsAsync(roleManager, AppRoles.Admin);
        await EnsureRoleExistsAsync(roleManager, AppRoles.Coach);
        await EnsureRoleExistsAsync(roleManager, AppRoles.Member);

        string? adminEmail = configuration["SeedAdmin:Email"];
        string? adminPassword = configuration["SeedAdmin:Password"];
        string? adminFullName = configuration["SeedAdmin:FullName"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword) ||
            string.IsNullOrWhiteSpace(adminFullName))
        {
            return;
        }

        ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail.Trim(),
                Email = adminEmail.Trim(),
                FullName = adminFullName.Trim(),
                EmailConfirmed = true
            };

            IdentityResult createResult = await userManager.CreateAsync(
                adminUser,
                adminPassword);

            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
        }

        User? domainUser = groupManager.FindUserByIdentityUserId(adminUser.Id);

        if (domainUser == null)
        {
            try
            {
                groupManager.CreateOrConnectIdentityUser(
                    adminUser.FullName,
                    adminUser.Email ?? adminEmail,
                    adminUser.Id);
            }
            catch
            {
                // Do not crash the app if the domain user already exists.
            }
        }

        domainUser = groupManager.FindUserByIdentityUserId(adminUser.Id);

        if (domainUser != null)
        {
            groupManager.SetUserCoachStatus(domainUser.Id, true);
        }
    }

    /// <summary>
    /// Creates a role when it does not already exist.
    /// </summary>
    /// <param name="roleManager">The ASP.NET Identity role manager.</param>
    /// <param name="roleName">The name of the role to ensure.</param>
    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        bool roleExists = await roleManager.RoleExistsAsync(roleName);

        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
