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
            try
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

                    _attendanceManager.MarkAttendance(
                        session.Id,
                        member.Id,
                        AttendanceStatus.Going,
                        "I will be there.");
                }

                List<User> users = _groupManager.GetAllUsers();
                List<Group> groups = _groupManager.GetAllGroups();
                List<PracticeSession> sessions = _practiceManager.GetAllSessions();

                AttendanceSummary? summary = null;

                if (sessions.Count > 0)
                {
                    summary = _attendanceManager.GetSummary(sessions[0].Id);
                }

                ViewBag.UserCount = users.Count;
                ViewBag.GroupCount = groups.Count;
                ViewBag.SessionCount = sessions.Count;
                ViewBag.GoingCount = summary?.GoingCount ?? 0;
                ViewBag.NotGoingCount = summary?.NotGoingCount ?? 0;
                ViewBag.MaybeCount = summary?.MaybeCount ?? 0;
                ViewBag.NoResponseCount = summary?.NoResponseCount ?? 0;
                ViewBag.TestResult = "Test data was created and loaded successfully.";
            }
            catch (Exception exception)
            {
                ViewBag.TestResult = "Test failed: " + exception.Message;
                ViewBag.UserCount = 0;
                ViewBag.GroupCount = 0;
                ViewBag.SessionCount = 0;
                ViewBag.GoingCount = 0;
                ViewBag.NotGoingCount = 0;
                ViewBag.MaybeCount = 0;
                ViewBag.NoResponseCount = 0;
            }

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
