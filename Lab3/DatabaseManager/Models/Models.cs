
namespace DatabaseManager.Models;

public record Event
{
    public required Guid Id { get; set; }

    public required Guid ArticleId { get; set; }
    public required string Action { get; set; } = "";
    public required string Value { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}

public record Article
{
    public required Guid ArticleId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "Empty description.";
}
