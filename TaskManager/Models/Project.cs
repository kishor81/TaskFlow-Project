namespace TaskManager.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key linking this project to its owner
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Navigation property — one project has many tasks
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
