using Core.Common.Logger;

namespace BB.API.Logging;

public class RabbitMqLoggerProvider : ILoggerProvider
{
    private readonly IRemoteLoggerFactory _factory;
    public RabbitMqLoggerProvider(IRemoteLoggerFactory factory)
    {
        _factory = factory;
    }

    public ILogger CreateLogger(string categoryName)
    {
        var logger = _factory.CreateAsync().GetAwaiter().GetResult();
        return new RabbitMqLoggerAdapter(logger);
    }

    public void Dispose() { }
}

public class RabbitMqLoggerAdapter : ILogger
{
    private readonly IRemoteLogger _logger;

    public RabbitMqLoggerAdapter(IRemoteLogger logger)
    {
        _logger = logger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        if (exception != null)
            message += $"\n{exception}";

        var task = logLevel switch
        {
            LogLevel.Debug       => _logger.LogDebugAsync(message),
            LogLevel.Warning     => _logger.LogWarningAsync(message),
            LogLevel.Error       => _logger.LogErrorAsync(message),
            LogLevel.Critical    => _logger.LogCriticalAsync(message),
            _                    => _logger.LogInfoAsync(message)
        };

        task.GetAwaiter().GetResult();
    }
    
}