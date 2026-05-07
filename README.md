# BBSolutions

.NET 10 Web API with JWT authentication, EF Core (SQL Server), and a `src` / `tests` solution layout.

## Quick start

```bash
# Restore, build, test
dotnet restore BBSolutions.slnx
dotnet build BBSolutions.slnx -c Release
dotnet test BBSolutions.slnx -c Release
```

Configure `src/BB.API/appsettings.Development.json` (or user secrets) with a valid `Jwt:Secret` (at least 32 characters) and a working `ConnectionStrings:DefaultConnection` for SQL Server.

```bash
dotnet user-secrets set "Jwt:Secret" "<your-key-at-least-32-chars>" --project src/BB.API
```

Run the API:

```bash
dotnet run --project src/BB.API
```

## Documentation

| Document | Description |
|----------|-------------|
| [docs/DATABASE_AND_MIGRATIONS.md](docs/DATABASE_AND_MIGRATIONS.md) | EF Core migrations, applying updates, and SQL Server notes |
| [docs/USER_ACCOUNT_SECURITY.md](docs/USER_ACCOUNT_SECURITY.md) | User model fields, login lockout, and API examples |

## Docker

From the repository root (build context is `.`):

```bash
docker compose build bb.api
docker compose up
```

## Solution layout

- `src/BB.API` — ASP.NET Core host
- `src/BB.Domain` — entities, DTOs, interfaces
- `src/BB.Infrastructure` — EF Core, repositories, security helpers
- `tests/BB.Tests.Unit` / `tests/BB.Tests.Integration` — xUnit tests

## CI

GitHub Actions runs `dotnet restore`, `build`, and `test` on `BBSolutions.slnx` for pushes and pull requests to `master`.
