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
        private readonly AttendanceManager _attendanceManager;

        public HomeController(
            GroupManager groupManager,
            PracticeManager practiceManager,
            AttendanceManager attendanceManager)
        {
            _groupManager = groupManager;
            _practiceManager = practiceManager;
            _attendanceManager = attendanceManager;
        }

        public IActionResult Index()
        {
            if (_groupManager.GetAllUsers().Count == 0)
            {
                User coach = _groupManager.CreateUser("Demo Coach", "coach@example.com", true);
                User member = _groupManager.CreateUser("Demo Member", "member@example.com");

                Group group = _groupManager.CreateGroup("Basketball Team", "Demo group for testing.");

                _groupManager.AddMemberToGroup(group.Id, coach.Id, GroupRole.Coach);
                _groupManager.AddMemberToGroup(group.Id, member.Id, GroupRole.Member);

                PracticeSession session = _practiceManager.CreateSession(
                    group.Id,
                    "Evening Practice",
                    DateTime.Now.AddDays(1),
                    DateTime.Now.AddDays(1).AddHours(2),
                    "Sports Hall",
                    10);

                _attendanceManager.MarkAttendance(session.Id, member.Id, AttendanceStatus.Going);
            }

            ViewBag.UserCount = _groupManager.GetAllUsers().Count;
            ViewBag.GroupCount = _groupManager.GetAllGroups().Count;
            ViewBag.SessionCount = _practiceManager.GetAllSessions().Count;

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
