using SquadCheck.Models;

namespace SquadCheck.Models.ViewModels;

public class GroupDetailsViewModel
{
    public Group Group { get; set; } = new();

    public List<User> Users { get; set; } = new();

    public List<GroupMember> Members { get; set; } = new();

    public List<PracticeSession> PracticeSessions { get; set; } = new();
}
