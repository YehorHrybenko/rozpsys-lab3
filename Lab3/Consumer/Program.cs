using DatabaseManager;
using DatabaseManager.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

using var context = new AppDbContext();
context.Database.EnsureCreated();

var articles = new Articles(context);

context.Events.ExecuteDelete();

Func<string, string?> env = Environment.GetEnvironmentVariable;

var connectionFactory = new ConnectionFactory
{
    HostName = env("RMQ_HOST")!,
    Port = int.Parse(env("RMQ_PORT")!),
    UserName = env("RMQ_USERNAME")!,
    Password = env("RMQ_PASSWORD")!,
};

using var connection = await connectionFactory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "events", durable: false, exclusive: false, autoDelete: false);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();

    var @event = JsonSerializer.Deserialize<Event>(body)!;

    Console.WriteLine($"Received event: {@event}");

    articles.Apply(@event);

    context.Events.Add(@event);

    context.SaveChanges();

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync("events", autoAck: true, consumer: consumer);

Console.ReadLine();
