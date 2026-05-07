# User account model and login security

This document describes **user-related persistence**, **login lockout**, and **API examples** for the BBSolutions API (`api/v1`).

## User entity (domain)

`BB.Domain.Entities.User` extends `AuditableSoftDeleteEntity` (created/updated/deleted stamps and soft delete). Important columns:

| Field | Purpose |
|-------|---------|
| `Email` | Unique among non-deleted users (filtered index) |
| `PasswordHash` | BCrypt hash; never exposed on API responses |
| `EmailConfirmed` / `EmailConfirmedAt` | Email verification state; new registrations start unconfirmed unless you set them elsewhere |
| `AccessFailedCount` | Failed password attempts since last successful login |
| `LockoutEnd` | UTC-based lockout end; when in the future, login is rejected |
| `PasswordChangedAt` | Set when a password is first hashed at user creation |
| `LastLoginAt` | Updated on each successful login |
| `RowVersion` | SQL Server `rowversion` for optimistic concurrency |

**Note:** Successful login does **not** currently require `EmailConfirmed`. You can add that rule in `AuthService` when you introduce a confirmation workflow.

## Lockout policy

Implemented in `BB.Infrastructure.Security.LoginLockout` and enforced in `AuthService`:

- **Maximum failed attempts:** 5  
- **Lockout duration:** 15 minutes after the 5th failed attempt  

Flow:

1. If `LockoutEnd > UtcNow`, login returns **unauthorized** without verifying the password.
2. Wrong password: increment `AccessFailedCount`, set `LockoutEnd` when threshold is reached, save.
3. Correct password: reset `AccessFailedCount` and `LockoutEnd`, set `LastLoginAt`, save, issue JWT.

## API versioning

Controllers use `Asp.Versioning` with URL segment `v{version:apiVersion}` (default **1.0**). Examples below use **`/api/v1/...`**.

## Examples (HTTP)

Replace `BASE` with your origin (e.g. `https://localhost:7xxx`).

### Register / create user (authenticated admin flow)

Create user is protected by JWT in this API. Example payload:

```http
POST BASE/api/v1/user
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "firstName": "Ada",
  "lastName": "Lovelace",
  "email": "ada@example.com",
  "password": "SecurePass123!"
}
```

**201 Created** returns a `UserResponse` including `emailConfirmed` (typically `false` for new users), `emailConfirmedAt`, `lastLoginAt`, and audit fields — **never** a password.

### Login

```http
POST BASE/api/v1/auth/login
Content-Type: application/json

{
  "email": "admin@bb.com",
  "password": "YourPassword"
}
```

**200 OK** example shape:

```json
{
  "token": "<jwt>",
  "expiresAt": "2026-05-08T12:34:56Z"
}
```

**401 Unauthorized** — unknown email, wrong password, or account locked out.

### Using the token

```http
GET BASE/api/v1/user?page=1&pageSize=10
Authorization: Bearer <jwt>
```

### Lockout behavior (integration coverage)

After **five** consecutive failed logins for the same user, further attempts return **401** until `LockoutEnd` passes, **even if the password is correct**. A successful login clears lockout state for the next attempts.

## Configuration reference

| Setting | Location | Notes |
|---------|-----------|--------|
| JWT signing | `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` | Secret must be ≥ 32 chars |
| SQL Server | `ConnectionStrings:DefaultConnection` | Used by EF at runtime and by `dotnet ef` |
| Rate limiting (`auth` policy) | `Program.cs` | Relaxed in `Testing`; fixed window in other environments |

## Related files

- `src/BB.Infrastructure/Repositories/AuthService.cs` — login and JWT issuance  
- `src/BB.Infrastructure/Repositories/UserRepository.cs` — persistence for login counters  
- `src/BB.Infrastructure/Data/AppDbContext.cs` — Fluent configuration and seed data  
- `src/BB.Domain/DTO/UserRequests.cs` — `UserResponse` shape for clients  
