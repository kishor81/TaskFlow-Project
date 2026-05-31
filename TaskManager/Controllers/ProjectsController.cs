using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.DTOs;
using TaskManager.Models;

namespace TaskManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Every endpoint here requires a valid JWT token
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    // Helper: read the user's ID from the JWT claims
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get all projects for the logged-in user</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
    {
        var userId = GetUserId();

        var projects = await _db.Projects
            .Where(p => p.UserId == userId)
            .Include(p => p.Tasks)            // Eagerly load tasks so we can count them
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectResponse(
                p.Id, p.Name, p.Description, p.CreatedAt, p.Tasks.Count))
            .ToListAsync();

        return Ok(projects);
    }

    /// <summary>Get a single project by ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponse>> GetProject(int id)
    {
        var userId = GetUserId();
        var project = await _db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (project is null) return NotFound();

        return Ok(new ProjectResponse(
            project.Id, project.Name, project.Description,
            project.CreatedAt, project.Tasks.Count));
    }

    /// <summary>Create a new project</summary>
    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> CreateProject(CreateProjectRequest req)
    {
        var project = new Project
        {
            Name = req.Name,
            Description = req.Description,
            UserId = GetUserId()
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var response = new ProjectResponse(
            project.Id, project.Name, project.Description, project.CreatedAt, 0);

        // CreatedAtAction returns 201 with a Location header pointing to the new resource
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, response);
    }

    /// <summary>Update an existing project</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequest req)
    {
        var userId = GetUserId();
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (project is null) return NotFound();

        project.Name = req.Name;
        project.Description = req.Description;
        await _db.SaveChangesAsync();

        return NoContent(); // 204 — success with no body
    }

    /// <summary>Delete a project (and all its tasks via cascade)</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var userId = GetUserId();
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (project is null) return NotFound();

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
