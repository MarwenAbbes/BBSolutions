namespace BB.Domain.Entities;

/// <summary>
/// UTC audit stamps and soft-delete flag. Maintained by EF save interceptor.
/// </summary>
public abstract class AuditableSoftDeleteEntity
{
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
