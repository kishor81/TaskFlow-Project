// DTOs (Data Transfer Objects) are simple classes that define
// exactly what data comes IN from requests and goes OUT in responses.
// We never expose our raw Model (which has PasswordHash etc.) directly.

namespace TaskManager.DTOs;

// --- Auth ---
public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Username, string Email);

// --- Projects ---
public record CreateProjectRequest(string Name, string Description);
public record UpdateProjectRequest(string Name, string Description);

public record ProjectResponse(
    int Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    int TaskCount
);

// --- Tasks ---
public record CreateTaskRequest(
    string Title,
    string Description,
    string Priority,   // "Low" | "Medium" | "High"
    DateTime? DueDate
);

public record UpdateTaskRequest(
    string Title,
    string Description,
    string Priority,
    bool IsCompleted,
    DateTime? DueDate
);

public record TaskResponse(
    int Id,
    string Title,
    string Description,
    bool IsCompleted,
    string Priority,
    DateTime? DueDate,
    DateTime CreatedAt
);
