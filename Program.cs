using SquadCheck.Data;
using SquadCheck.Managers;

namespace SquadCheck
{
    /// <summary>
    /// Starts the SquadCheck web application and configures its services and request pipeline.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Configures dependency injection, middleware, routes, and then runs the application.
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
