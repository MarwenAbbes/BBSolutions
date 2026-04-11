using Core.Common.Logger;
using RabbitMQ.Client;

namespace Core.Common.RabbitMQ;

public class RabbitMqLoggerFactory : IRemoteLoggerFactory
{
    private readonly RabbitMqOptions _options;

    public RabbitMqLoggerFactory(RabbitMqOptions options)
    {
        _options = options;
    }

    public async Task<IRemoteLogger> CreateAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port     = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
        };

        var connection = await factory.CreateConnectionAsync();
        var channel    = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true);

        return new RabbitMqLogger(connection, channel, _options.Exchange, _options.SourceApplication);
    }
}