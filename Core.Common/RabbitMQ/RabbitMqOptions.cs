namespace Core.Common.RabbitMQ;

public class RabbitMqOptions
{
    public string SourceApplication { get; set; } = "No_application";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "logs";
    public string ExchangeName { get; set; } = "logs.exchange";
    public string RoutingKey { get; set; } = "logs.#";
    public string Exchange { get; set; } = "logs.exchange";
}