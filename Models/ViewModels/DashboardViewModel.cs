using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Data shown on the home dashboard.
/// </summary>
public class DashboardViewModel
{
    public User CurrentUser { get; set; } = new();

    public bool IsAdmin { get; set; }

    public bool IsCoach { get; set; }

    public int GroupCount { get; set; }

    public int UpcomingPracticeCount { get; set; }

    public int MissingResponseCount { get; set; }

    public DashboardPracticeCardViewModel? NextPractice { get; set; }

    public List<DashboardPracticeCardViewModel> UpcomingPractices { get; set; } = new();

    public List<DashboardPracticeCardViewModel> CoachPractices { get; set; } = new();
}
