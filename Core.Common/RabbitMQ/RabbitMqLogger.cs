using System.Text.Json;
using Core.Common.Logger;
using RabbitMQ.Client;

namespace Core.Common.RabbitMQ;

public class RabbitMqLogger : IRemoteLogger
{
    private readonly IChannel _channel;
    private readonly IConnection _connection;
    private readonly string _exchange;
    private readonly string _source;

    internal RabbitMqLogger(IConnection connection, IChannel channel,
        string exchange, string source)
    {
        _connection = connection;
        _channel    = channel;
        _exchange   = exchange;
        _source     = source;
    }
    public Task LogInfoAsync(string message)     => LogAsync(message, "Information");
    public Task LogDebugAsync(string message)    => LogAsync(message, "Debug");
    public Task LogWarningAsync(string message)  => LogAsync(message, "Warning");
    public Task LogErrorAsync(string message)    => LogAsync(message, "Error");
    public Task LogCriticalAsync(string message) => LogAsync(message, "Critical");

    public Task LogAsync(string message, string level = "Information",
        Dictionary<string, object>? properties = null)
    {
        var logMsg = new LogMessage
        {
            Level      = level,
            Message    = message,
            Source     = _source,
            Timestamp  = DateTime.UtcNow,
            Properties = properties
        };

        var body       = new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(logMsg));
        var routingKey = $"logs.{_source.ToLower()}";

        return _channel.BasicPublishAsync(
            exchange:   _exchange,
            routingKey: routingKey,
            body:       body).AsTask();
    }
    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
        _channel.Dispose();
        _connection.Dispose();
    }
}