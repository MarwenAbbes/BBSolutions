# Database and EF Core migrations

This project uses **EF Core 10** with **SQL Server** in normal environments and an **in-memory** database when `ASPNETCORE_ENVIRONMENT=Testing` (integration tests).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) reachable from your machine (local instance, Docker, or Azure)
- EF CLI tools (install once per machine):

```bash
dotnet tool install --global dotnet-ef
# or update
dotnet tool update --global dotnet-ef
```

## Connection string

Set `ConnectionStrings:DefaultConnection` in `src/BB.API/appsettings.json`, environment variables, or user secrets. Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=BBDb;User Id=sa;Password=***;TrustServerCertificate=True"
}
```

When using Docker Compose in this repo, align the server name and credentials with `compose.yaml` (e.g. server `sqlserver` inside the compose network, `localhost` from the host).

## Apply migrations (update the database)

From the **repository root**, run:

```bash
dotnet ef database update --project src/BB.Infrastructure --startup-project src/BB.API
```

This uses the startup project’s configuration to resolve the connection string and applies any pending migrations (including `Users` security columns such as `RowVersion`, `LockoutEnd`, etc.).

### Verify applied migrations

You can inspect the `__EFMigrationsHistory` table in SQL Server or list migrations:

```bash
dotnet ef migrations list --project src/BB.Infrastructure --startup-project src/BB.API
```

## Add a new migration

After changing the model in `AppDbContext` or entities:

```bash
dotnet ef migrations add <MigrationName> --project src/BB.Infrastructure --startup-project src/BB.API
```

Review the generated migration under `src/BB.Infrastructure/Migrations/`, then apply:

```bash
dotnet ef database update --project src/BB.Infrastructure --startup-project src/BB.API
```

## Remove the last migration (not yet applied)

```bash
dotnet ef migrations remove --project src/BB.Infrastructure --startup-project src/BB.API
```

## Generate SQL script (review or DBA handoff)

```bash
dotnet ef migrations script --project src/BB.Infrastructure --startup-project src/BB.API --output migrate.sql
```

Optional `from` / `to` migration names limit the script range; see `dotnet ef migrations script --help`.

## Runtime behavior

- **Development / Production:** `Program.cs` calls `Database.Migrate()` on startup so the database is brought up to date automatically when the app starts (not used in `Testing`).
- **Testing:** `UseInMemoryDatabase("TestDb")` — migrations are **not** applied; the model is created in memory for tests.

## Troubleshooting

| Symptom | What to check |
|--------|----------------|
| Cannot connect | Connection string, firewall, SQL Server running, `TrustServerCertificate` for dev certs |
| Pending model changes warning | Model out of sync with snapshot — add a migration or fix the model |
| Login / column errors after pull | Run `dotnet ef database update` with the same connection your app uses |

For more EF patterns (tracking, retries), see the project’s `.cursor/skills/efcore-patterns` reference if present.
