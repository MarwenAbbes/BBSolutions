using BB.Domain.Entities;
using BB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BB.Infrastructure.Data;

public sealed class AuditSoftDeleteInterceptor(
    ICurrentUserAccessor currentUser,
    ILogger<AuditSoftDeleteInterceptor> logger) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Process(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Process(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Process(DbContext? context)
    {
        if (context is null) return;

        var actorEmail = currentUser.Email;
        var actorId = currentUser.UserId;
        var utc = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<AuditableSoftDeleteEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                        entry.Entity.CreatedAt = utc;
                    entry.Entity.CreatedBy ??= actorEmail;

                    logger.LogInformation(
                        "Audit create {EntityType} Id={EntityId} Email={ActorEmail} UserId={ActorId}",
                        entry.Metadata.ClrType.Name,
                        FormatKey(entry),
                        actorEmail,
                        actorId);
                    break;

                case EntityState.Modified when !entry.Entity.IsDeleted:
                    entry.Entity.UpdatedAt = utc;
                    entry.Entity.UpdatedBy = actorEmail;

                    logger.LogInformation(
                        "Audit update {EntityType} Id={EntityId} Email={ActorEmail} UserId={ActorId}",
                        entry.Metadata.ClrType.Name,
                        FormatKey(entry),
                        actorEmail,
                        actorId);
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = utc;
                    entry.Entity.DeletedBy = actorEmail;
                    entry.Entity.UpdatedAt = utc;
                    entry.Entity.UpdatedBy = actorEmail;

                    logger.LogInformation(
                        "Audit soft-delete {EntityType} Id={EntityId} Email={ActorEmail} UserId={ActorId}",
                        entry.Metadata.ClrType.Name,
                        FormatKey(entry),
                        actorEmail,
                        actorId);
                    break;
            }
        }
    }

    private static string FormatKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return "?";
        return string.Join(',', key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? "?"));
    }
}
