namespace Core.Common.Logger;

public interface IRemoteLogger : IAsyncDisposable
{
    Task LogAsync(string message, string level = "Information",
        Dictionary<string, object>? properties = null);

    Task LogInfoAsync(string message);
    Task LogDebugAsync(string message);
    Task LogWarningAsync(string message);
    Task LogErrorAsync(string message);
    Task LogCriticalAsync(string message);
}