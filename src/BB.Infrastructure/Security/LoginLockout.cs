namespace BB.Infrastructure.Security;

internal static class LoginLockout
{
    internal const int MaxFailedAccessAttempts = 5;
    internal static readonly TimeSpan DefaultLockoutDuration = TimeSpan.FromMinutes(15);
}
