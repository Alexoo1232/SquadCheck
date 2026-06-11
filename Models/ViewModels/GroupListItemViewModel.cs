using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

/// <summary>
/// Data shown for one group on the group list page.
/// </summary>
public class GroupListItemViewModel
{
    public Group Group { get; set; } = new Group();

    public int MemberCount { get; set; }

    public int PracticeCount { get; set; }

    public int UpcomingPracticeCount { get; set; }

    public bool CanManageGroup { get; set; }
}
