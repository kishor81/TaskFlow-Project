# Task Manager API

A RESTful API built with ASP.NET Core 8, Entity Framework Core, and JWT Authentication.
Built as an internship portfolio project.

## Tech Stack

- **ASP.NET Core 8** — Web framework
- **Entity Framework Core** — ORM (maps C# classes to database tables)
- **SQLite** — Lightweight database (no setup required)
- **JWT Bearer Auth** — Stateless authentication via signed tokens
- **BCrypt** — Secure password hashing
- **Swagger / OpenAPI** — Auto-generated interactive API documentation

## Project Structure

```
TaskManager/
├── Controllers/         # HTTP endpoints — handle requests and return responses
│   ├── AuthController.cs
│   ├── ProjectsController.cs
│   └── TasksController.cs
├── Data/
│   └── AppDbContext.cs  # EF Core context — bridges C# models and the database
├── DTOs/
│   └── Dtos.cs          # Request/response shapes (what goes in and out of the API)
├── Models/
│   ├── User.cs          # Database entity
│   ├── Project.cs       # Database entity
│   └── TaskItem.cs      # Database entity
├── Services/
│   └── TokenService.cs  # Generates JWT tokens
└── Program.cs           # App entry point — wires everything together
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)

### Run the API

```bash
# 1. Restore packages
dotnet restore

# 2. Run the app (DB is created automatically)
dotnet run
```

Open your browser at **http://localhost:5000/swagger** to explore the API.

## API Endpoints

### Auth (no token required)

| Method | URL | Description |
|--------|-----|-------------|
| POST | `/api/auth/register` | Create a new account |
| POST | `/api/auth/login` | Login and receive a JWT token |

### Projects (JWT required)

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/projects` | List all your projects |
| GET | `/api/projects/{id}` | Get a single project |
| POST | `/api/projects` | Create a project |
| PUT | `/api/projects/{id}` | Update a project |
| DELETE | `/api/projects/{id}` | Delete a project |

### Tasks (JWT required)

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/api/projects/{id}/tasks` | List tasks (supports filtering + sorting) |
| GET | `/api/projects/{id}/tasks/{taskId}` | Get a single task |
| POST | `/api/projects/{id}/tasks` | Create a task |
| PUT | `/api/projects/{id}/tasks/{taskId}` | Update / complete a task |
| DELETE | `/api/projects/{id}/tasks/{taskId}` | Delete a task |

#### Task filtering query params

- `?completed=true` — Only completed tasks
- `?priority=High` — Filter by priority (Low / Medium / High)
- `?sort=duedate` — Sort by due date (default: created date)

## How to Test

1. Open **http://localhost:5000/swagger**
2. Call `POST /api/auth/register` with `{ "username": "alice", "email": "alice@example.com", "password": "secret123" }`
3. Copy the `token` from the response
4. Click **Authorize** (top right), paste `Bearer <your-token>`
5. Now you can call the protected endpoints

## Key Concepts Demonstrated

- **REST API design** — proper HTTP verbs, status codes (200/201/204/400/401/404)
- **ORM with EF Core** — models, relationships, LINQ queries, cascade deletes
- **JWT Authentication** — stateless auth, claims, token validation
- **Security** — BCrypt password hashing, ownership checks (users only see their own data)
- **DTOs** — separating API contracts from database models
- **Dependency Injection** — services registered and injected automatically
- **Clean architecture** — Controllers → Services → Data layer
