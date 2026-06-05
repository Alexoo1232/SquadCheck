using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

public class GroupListItemViewModel
{
    public Group Group { get; set; } = new Group();

    public int MemberCount { get; set; }

    public int PracticeCount { get; set; }

    public int UpcomingPracticeCount { get; set; }
}
