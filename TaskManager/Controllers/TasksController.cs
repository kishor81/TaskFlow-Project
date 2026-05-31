using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.DTOs;
using TaskManager.Models;

namespace TaskManager.Controllers;

// Nested route: /api/projects/{projectId}/tasks
[ApiController]
[Route("api/projects/{projectId}/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Helper: verify the project exists and belongs to this user
    private async Task<Project?> GetOwnedProject(int projectId) =>
        await _db.Projects.FirstOrDefaultAsync(
            p => p.Id == projectId && p.UserId == GetUserId());

    /// <summary>Get all tasks in a project, with optional filters</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetTasks(
        int projectId,
        [FromQuery] bool? completed,         // ?completed=true
        [FromQuery] string? priority,        // ?priority=High
        [FromQuery] string? sort = "created" // ?sort=duedate
    )
    {
        if (await GetOwnedProject(projectId) is null) return NotFound("Project not found.");

        var query = _db.Tasks.Where(t => t.ProjectId == projectId).AsQueryable();

        // Optional filtering
        if (completed.HasValue)
            query = query.Where(t => t.IsCompleted == completed.Value);

        if (!string.IsNullOrEmpty(priority) &&
            Enum.TryParse<TaskPriority>(priority, true, out var p))
            query = query.Where(t => t.Priority == p);

        // Optional sorting
        query = sort?.ToLower() switch
        {
            "duedate" => query.OrderBy(t => t.DueDate),
            "priority" => query.OrderByDescending(t => t.Priority),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var tasks = await query
            .Select(t => new TaskResponse(
                t.Id, t.Title, t.Description, t.IsCompleted,
                t.Priority.ToString(), t.DueDate, t.CreatedAt))
            .ToListAsync();

        return Ok(tasks);
    }

    /// <summary>Get a single task</summary>
    [HttpGet("{taskId}")]
    public async Task<ActionResult<TaskResponse>> GetTask(int projectId, int taskId)
    {
        if (await GetOwnedProject(projectId) is null) return NotFound("Project not found.");

        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task is null) return NotFound("Task not found.");

        return Ok(new TaskResponse(
            task.Id, task.Title, task.Description, task.IsCompleted,
            task.Priority.ToString(), task.DueDate, task.CreatedAt));
    }

    /// <summary>Create a task in a project</summary>
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask(
        int projectId, CreateTaskRequest req)
    {
        if (await GetOwnedProject(projectId) is null) return NotFound("Project not found.");

        if (!Enum.TryParse<TaskPriority>(req.Priority, true, out var priority))
            return BadRequest("Priority must be Low, Medium, or High.");

        var task = new TaskItem
        {
            Title = req.Title,
            Description = req.Description,
            Priority = priority,
            DueDate = req.DueDate,
            ProjectId = projectId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        var response = new TaskResponse(
            task.Id, task.Title, task.Description, task.IsCompleted,
            task.Priority.ToString(), task.DueDate, task.CreatedAt);

        return CreatedAtAction(nameof(GetTask),
            new { projectId, taskId = task.Id }, response);
    }

    /// <summary>Update a task (including marking complete)</summary>
    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask(
        int projectId, int taskId, UpdateTaskRequest req)
    {
        if (await GetOwnedProject(projectId) is null) return NotFound("Project not found.");

        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task is null) return NotFound("Task not found.");

        if (!Enum.TryParse<TaskPriority>(req.Priority, true, out var priority))
            return BadRequest("Priority must be Low, Medium, or High.");

        task.Title = req.Title;
        task.Description = req.Description;
        task.IsCompleted = req.IsCompleted;
        task.Priority = priority;
        task.DueDate = req.DueDate;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Delete a task</summary>
    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int projectId, int taskId)
    {
        if (await GetOwnedProject(projectId) is null) return NotFound("Project not found.");

        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task is null) return NotFound("Task not found.");

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
