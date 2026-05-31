namespace TaskManager.Models;

// We name this TaskItem (not Task) to avoid conflict with System.Threading.Tasks.Task
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key linking this task to a project
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}

// Enum stored as an integer in the DB
public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}
