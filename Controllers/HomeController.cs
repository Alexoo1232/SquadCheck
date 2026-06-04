using Microsoft.AspNetCore.Mvc;
using SquadCheck.Managers;
using SquadCheck.Models;
using System.Diagnostics;

namespace SquadCheck.Controllers
{
    public class HomeController : Controller
    {
        private readonly GroupManager _groupManager;
        private readonly PracticeManager _practiceManager;

        public HomeController(
            GroupManager groupManager,
            PracticeManager practiceManager)
        {
            _groupManager = groupManager;
            _practiceManager = practiceManager;
        }

        public IActionResult Index()
        {
            ViewBag.GroupCount = _groupManager.GetAllGroups().Count;
            ViewBag.UpcomingPracticeCount = _practiceManager.GetUpcomingSessions().Count;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
