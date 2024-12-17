using EventGenerator.Extensions;
using DatabaseManager;
using DatabaseManager.Models;
using EventGenerator.EventPublisher;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


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

app.MapGet("/articles", () =>
{
    using var context = new AppDbContext();

    var articles = context.State.ToList();
    articles.SortBy(a => a.CreatedAt);

    return JsonSerializer.Serialize(articles);
})
.WithName("GetArticles")
.WithOpenApi();

app.MapGet("/events", () =>
{
    using var context = new AppDbContext();
    var allEvents = context.Events.ToList();
    allEvents.SortBy(a => a.CreatedAt);

    return JsonSerializer.Serialize(allEvents);
})
.WithName("GetEvents")
.WithOpenApi();

app.MapGet("/create", (string title = "EmptyTitle") =>
{
    var @event = new Event
    {
        Id = Guid.NewGuid(),
        ArticleId = Guid.NewGuid(),
        Action = "create",
        Value = title,
        CreatedAt = DateTime.UtcNow
    };

    publisher.PublishEvent(JsonSerializer.Serialize(@event));

    return $"Added creation event: {@event}";
})
.WithName("CreateArticle")
.WithOpenApi();

app.MapGet("/update", (Guid ArticleId, string description = "EmptyTitle") =>
{
    var @event = new Event
    {
        Id = Guid.NewGuid(),
        ArticleId = ArticleId,
        Action = "update",
        Value = description,
        CreatedAt = DateTime.UtcNow
    };

    publisher.PublishEvent(JsonSerializer.Serialize(@event));

    return $"Added update event: {@event}";
})
.WithName("UpdateArticleDescription")
.WithOpenApi();

app.MapGet("/clear", () =>
{
    var @event = new Event
    {
        Id = Guid.NewGuid(),
        ArticleId = Guid.Empty,
        Action = "clear",
        Value = "",
        CreatedAt = DateTime.UtcNow
    };

    publisher.PublishEvent(JsonSerializer.Serialize(@event));

    return $"Added clear event: {@event}";
})
.WithName("ClearAllArticles")
.WithOpenApi();

app.Run();
