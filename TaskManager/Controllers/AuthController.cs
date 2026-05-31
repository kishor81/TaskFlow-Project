using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.DTOs;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Controllers;

// [ApiController] enables automatic model validation and better error responses
// [Route] sets the base URL path: /api/auth
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    // ASP.NET's dependency injection automatically provides these
    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    /// <summary>Register a new account</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        // Check if email is already taken
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest("Email already in use.");

        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            // BCrypt hashes the password — NEVER store plain text passwords
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Username, user.Email));
    }

    /// <summary>Login and receive a JWT token</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

        // Verify the password against the stored hash
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Username, user.Email));
    }
}
