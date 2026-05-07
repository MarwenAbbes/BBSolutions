namespace BB.Domain.Interfaces;

/// <summary>
/// Scoped accessor for the authenticated user (HTTP). Null when anonymous or outside a request.
/// </summary>
public interface ICurrentUserAccessor
{
    string? Email { get; }
    string? UserId { get; }
}
