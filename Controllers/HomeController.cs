using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using System.Diagnostics;

namespace SquadCheck.Controllers
{
    /// <summary>
    /// Handles the home dashboard and standard error page.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly GroupManager _groupManager;
        private readonly PracticeManager _practiceManager;

        /// <summary>
        /// Creates a controller with the managers needed for dashboard counts.
        /// </summary>
        /// <param name="groupManager">Manager used to count groups.</param>
        /// <param name="practiceManager">Manager used to count upcoming practices.</param>
        public HomeController(
            GroupManager groupManager,
            PracticeManager practiceManager)
        {
            _groupManager = groupManager;
            _practiceManager = practiceManager;
        }

        /// <summary>
        /// Shows the dashboard with basic group and practice counts.
        /// </summary>
        /// <returns>The home dashboard view.</returns>
        public IActionResult Index()
        {
            List<Group> groups = _groupManager.GetAllGroups();
            List<PracticeSession> upcomingPracticeSessions = _practiceManager.GetUpcomingSessions();

            ViewBag.GroupCount = groups.Count;
            ViewBag.UpcomingPracticeCount = upcomingPracticeSessions.Count;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        /// <summary>
        /// Shows the error page with the current request id.
        /// </summary>
        /// <returns>The error view.</returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
