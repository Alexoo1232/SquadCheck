using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using System.Diagnostics;

namespace SquadCheck.Controllers
{
    /// <summary>
    /// Controls the home page and error page.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly GroupManager _groupManager;
        private readonly PracticeManager _practiceManager;

        /// <summary>
        /// Gets the managers used for the home page counts.
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
        /// Shows the home dashboard.
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
        /// Shows the error page.
        /// </summary>
        /// <returns>The error view.</returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
