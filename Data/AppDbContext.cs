using Microsoft.EntityFrameworkCore;
using TimeTrackerWeb.Domain;

namespace TimeTrackerWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Tasks)
            .WithOne(t => t.Project!)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.TimeEntries)
            .WithOne(e => e.TaskItem!)
            .HasForeignKey(e => e.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}