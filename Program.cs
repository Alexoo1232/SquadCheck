using SquadCheck.Data;
using SquadCheck.Managers;

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
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IDataManager, DataManager>();
            builder.Services.AddScoped<GroupManager>();
            builder.Services.AddScoped<PracticeManager>();
            builder.Services.AddScoped<AttendanceManager>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
