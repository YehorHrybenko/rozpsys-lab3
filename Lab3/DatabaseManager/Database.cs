﻿namespace DatabaseManager;

using DatabaseManager.Models;
using Microsoft.EntityFrameworkCore;
using System;

public class AppDbContext : DbContext
{
    public DbSet<Event> Events { get; set; }
    public DbSet<Article> State { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=postgres;Database=events;Username=postgresuser;Password=postgrespassword");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("Articles");
            entity.HasKey(e => e.ArticleId);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}

public class Articles(AppDbContext context)
{
    private readonly AppDbContext context = context;

    public void Apply(Event @event)
    {
        switch (@event.Action)
        {
            case "create":
                CreateArticle(@event.ArticleId, @event.Value, @event.CreatedAt);
                break;
            case "update":
                EditArticle(@event.ArticleId, @event.Value);
                break;
            case "clear":
                ClearArticles();
                break;
        }
    }

    public List<Article> GetArticles() => [.. context.State];

    private void CreateArticle(Guid articleId, string value, DateTime CreatedAt)
    {
        context.State.Add(new Article { ArticleId = articleId, Title = value, CreatedAt = CreatedAt });
        context.SaveChanges();
    }

    private void EditArticle(Guid articleId, string value)
    {
        try
        {
            var article = context.State.Single(a => a.ArticleId == articleId);
            article.Description = value;
            context.SaveChanges();

            Console.WriteLine($"Updated {articleId} with description {value}");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Error: Article not found!");
        }
    }

    public void ClearArticles()
    {
        context.State.ExecuteDelete();
        context.SaveChanges();
    }
}
