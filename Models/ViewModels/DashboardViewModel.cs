using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

public class DashboardViewModel
{
    public int UserCount { get; set; }

    public int GroupCount { get; set; }

    public int PracticeSessionCount { get; set; }

    public int UpcomingPracticeSessionCount { get; set; }

    public List<PracticeSession> UpcomingPracticeSessions { get; set; } = new();
}
