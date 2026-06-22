using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SquadCheck.Data;
using SquadCheck.Managers;
using SquadCheck.Models;
using SquadCheck.Services.Email;
using SquadCheck.Services.Reminders;

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

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings"));

            builder.Services.Configure<ReminderSettings>(
                builder.Configuration.GetSection("ReminderSettings"));

            builder.Services.AddTransient<IEmailService, SmtpEmailService>();
            builder.Services.AddHostedService<PracticeReminderWorker>();
            
            builder.Services.AddScoped<GroupManager>();
            builder.Services.AddScoped<PracticeManager>();
            builder.Services.AddScoped<AttendanceManager>();

            WebApplication app = builder.Build();

            IdentitySeeder.SeedAsync(app.Services).GetAwaiter().GetResult();

            CultureInfo appCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = appCulture;
            CultureInfo.DefaultThreadCurrentUICulture = appCulture;

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(appCulture),
                SupportedCultures = new[] { appCulture },
                SupportedUICultures = new[] { appCulture }
            });

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
