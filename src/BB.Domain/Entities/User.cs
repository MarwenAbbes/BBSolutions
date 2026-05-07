namespace BB.Domain.Entities;

public class User : AuditableSoftDeleteEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public required string PasswordHash { get; set; }

    public bool EmailConfirmed { get; set; }
    public DateTime? EmailConfirmedAt { get; set; }

    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTime? PasswordChangedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
