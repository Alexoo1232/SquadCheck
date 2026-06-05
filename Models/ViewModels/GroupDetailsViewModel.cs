using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

public class GroupDetailsViewModel
{
    public Group Group { get; set; } = new Group();

    public List<User> Users { get; set; } = new List<User>();

    public List<GroupMember> Members { get; set; } = new List<GroupMember>();

    public List<PracticeSession> PracticeSessions { get; set; } = new List<PracticeSession>();
}
