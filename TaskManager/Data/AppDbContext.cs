using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Data;

// DbContext is EF Core's main class — it represents a session with the database
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Each DbSet maps to a table in the database
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enforce unique email constraint at the database level
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // One user -> many projects (cascade delete: deleting a user removes their projects)
        modelBuilder.Entity<Project>()
            .HasOne(p => p.User)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One project -> many tasks
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
