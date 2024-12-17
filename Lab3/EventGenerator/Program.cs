using EventGenerator.Extensions;
using DatabaseManager;
using DatabaseManager.Models;
using EventGenerator.EventPublisher;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

using var context = new AppDbContext();
context.Database.EnsureCreated();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var publisher = new EventPublisher(app.Configuration);

// TODO: Remove test code

var article1Id = Guid.NewGuid();

var @event = new Event
{
    Id = Guid.NewGuid(),
    ArticleId = article1Id,
    Action = "create",
    Value = "Article1Title",
    CreatedAt = DateTime.UtcNow
};

publisher.PublishEvent(JsonSerializer.Serialize(@event));

var @event2 = new Event
{
    Id = Guid.NewGuid(),
    ArticleId = Guid.NewGuid(),
    Action = "create",
    Value = "Article2Title",
    CreatedAt = DateTime.UtcNow
};

publisher.PublishEvent(JsonSerializer.Serialize(@event2));

app.MapGet("/articles", () =>
{
    return JsonSerializer.Serialize(context.State.ToList());
})
.WithName("GetArticles")
.WithOpenApi();

app.MapGet("/events", () =>
{
    var allEvents = context.Events.ToList();
    allEvents.SortBy(a => a.CreatedAt);

    return JsonSerializer.Serialize(allEvents);
})
.WithName("GetEvents")
.WithOpenApi();

// TODO: add endpoints for crud

app.Run();
