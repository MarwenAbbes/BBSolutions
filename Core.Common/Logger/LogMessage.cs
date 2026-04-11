namespace Core.Common.Logger;

public class LogMessage
{
    public string Level { get; set; } = "Information";
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Properties { get; set; }
}