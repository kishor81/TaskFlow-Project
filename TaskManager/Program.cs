using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManager.Data;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);

// ── 1. DATABASE ───────────────────────────────────────────────────────────────
// Register EF Core with SQLite — the DB file will be created automatically
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=taskmanager.db"));

// ── 2. JWT AUTHENTICATION ─────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret not set in appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

// ── 3. CORS ───────────────────────────────────────────────────────────────────
// Allow the frontend (any local file or port) to call the API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "null"  // for file:// origins
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── 4. SERVICES ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<TokenService>();   // Scoped = one instance per HTTP request
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── 4. SWAGGER (API DOCS) ─────────────────────────────────────────────────────
// This generates the interactive documentation page at /swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager API",
        Version = "v1",
        Description = "A RESTful API for managing projects and tasks"
    });

    // Tell Swagger about our JWT auth so we can test protected endpoints
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── 5. AUTO-MIGRATE DATABASE ──────────────────────────────────────────────────
// Automatically create/update the DB schema on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // For dev — use Migrate() in production
}

// ── 6. MIDDLEWARE PIPELINE ────────────────────────────────────────────────────
// Order matters! Middleware runs top to bottom on every request.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
