using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SquadCheck.Data;
using SquadCheck.Managers;
using SquadCheck.Models;

namespace SquadCheck
{
    /// <summary>
    /// Sets up and starts the SquadCheck web app.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Registers services, sets up routing, and runs the app.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            string connectionString = builder.Configuration
                .GetConnectionString("SquadCheckConnection")
                ?? throw new InvalidOperationException("Connection string 'SquadCheckConnection' was not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });
            
            builder.Services.AddScoped<GroupManager>();
            builder.Services.AddScoped<PracticeManager>();
            builder.Services.AddScoped<AttendanceManager>();

            WebApplication app = builder.Build();

            IdentitySeeder.SeedAsync(app.Services).GetAwaiter().GetResult();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
