
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EventGenerator.EventPublisher;

public class EventPublisher
{
    private readonly string _queueName = "events";
    private readonly ConnectionFactory connectionFactory;

    public EventPublisher(IConfiguration config)
    {
        connectionFactory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"]!,
            Port = config.GetValue<int>("RabbitMQ:Port"),
            UserName = config["RabbitMQ:UserName"]!,
            Password = config["RabbitMQ:Password"]!,
        };
    }

    public async void PublishEvent(string messageBody)
    {
        Thread.Sleep(5000);

        try
        {
            using var connection = await connectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(messageBody);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _queueName,
                body: body
            );

            Console.WriteLine($"Event published: {messageBody}");
            return;
        }
        catch (BrokerUnreachableException)
        {
            Console.WriteLine($"RabbitMQ connection failed.");
        }
    }
}
