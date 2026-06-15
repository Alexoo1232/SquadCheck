using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SquadCheck.Models;

namespace SquadCheck.Data;

/// <summary>
/// EF Core database context for ASP.NET Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> DomainUsers => Set<User>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();

    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();

    public DbSet<Attendance> Attendances => Set<Attendance>();

    public DbSet<EmailNotificationLog> EmailNotificationLogs => Set<EmailNotificationLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.FullName)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(user => user.IdentityUserId)
                .HasMaxLength(450);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.HasIndex(user => user.IdentityUserId)
                .IsUnique();
        });

        builder.Entity<Group>(entity =>
        {
            entity.ToTable("Groups");

            entity.HasKey(group => group.Id);

            entity.Property(group => group.Name)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(group => group.Description)
                .HasMaxLength(300);

            entity.Property(group => group.JoinCode)
                .HasMaxLength(20);

            entity.Property(group => group.IsJoinCodeEnabled)
                .IsRequired();

            entity.HasIndex(group => group.Name)
                .IsUnique();

            entity.HasIndex(group => group.JoinCode)
                .IsUnique();
        });

        builder.Entity<GroupMember>(entity =>
        {
            entity.ToTable("GroupMembers");

            entity.HasKey(member => member.Id);

            entity.HasIndex(member => new
                {
                    member.GroupId,
                    member.UserId
                })
                .IsUnique();

            entity.Property(member => member.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(member => member.JoinedAt)
                .HasColumnType("timestamp without time zone");

            entity.HasOne(member => member.Group)
                .WithMany(group => group.Members)
                .HasForeignKey(member => member.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(member => member.User)
                .WithMany()
                .HasForeignKey(member => member.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PracticeSession>(entity =>
        {
            entity.ToTable("PracticeSessions");

            entity.HasKey(session => session.Id);

            entity.Property(session => session.Title)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(session => session.Location)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(session => session.StartTime)
                .HasColumnType("timestamp without time zone");

            entity.Property(session => session.EndTime)
                .HasColumnType("timestamp without time zone");

            entity.HasOne(session => session.Group)
                .WithMany()
                .HasForeignKey(session => session.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendances");

            entity.HasKey(attendance => attendance.Id);

            entity.HasIndex(attendance => new
                {
                    attendance.PracticeSessionId,
                    attendance.UserId
                })
                .IsUnique();

            entity.Property(attendance => attendance.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(attendance => attendance.Comment)
                .HasMaxLength(200);

            entity.Property(attendance => attendance.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            entity.HasOne(attendance => attendance.PracticeSession)
                .WithMany()
                .HasForeignKey(attendance => attendance.PracticeSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(attendance => attendance.User)
                .WithMany()
                .HasForeignKey(attendance => attendance.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EmailNotificationLog>(entity =>
        {
            entity.ToTable("EmailNotificationLogs");

            entity.HasKey(log => log.Id);

            entity.Property(log => log.NotificationType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(log => log.SentAt)
                .HasColumnType("timestamp without time zone");

            entity.HasIndex(log => new
                {
                    log.PracticeSessionId,
                    log.UserId,
                    log.NotificationType
                })
                .IsUnique();

            entity.HasOne(log => log.PracticeSession)
                .WithMany()
                .HasForeignKey(log => log.PracticeSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(log => log.User)
                .WithMany()
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
