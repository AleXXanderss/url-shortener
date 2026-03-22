using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Api.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace UrlShortener.Api.Services;

public class RedirectService
{
    private readonly AppDbContext _db;
    private readonly IDatabase _cache;
    private readonly ClickQueuePublisher _publisher;

    public RedirectService(
        AppDbContext db,
        IConnectionMultiplexer redis,
        ClickQueuePublisher publisher)
    {
        _db = db;
        _cache = redis.GetDatabase();
        _publisher = publisher;
    }

    public async Task<string?> GetOriginalUrlAsync(string code)
    {
        var cacheKey = $"link:{code}";

        // 1. Пытаемся взять из Redis
        try
        {
            var cached = await _cache.StringGetAsync(cacheKey);

            if (!cached.IsNullOrEmpty)
            {
                Console.WriteLine("CACHE HIT");
                return cached!;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"REDIS ERROR (GET): {ex.Message}");
        }

        // 2. Если нет в кеше — идём в БД
        Console.WriteLine("CACHE MISS");

        var link = await _db.Links
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.ShortCode == code);

        if (link == null)
            return null;

        var url = link.OriginalUrl;

        // 3. Пишем в Redis
        try
        {
            await _cache.StringSetAsync(
                cacheKey,
                url,
                TimeSpan.FromMinutes(10));

            Console.WriteLine("WRITTEN TO REDIS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"REDIS ERROR (SET): {ex.Message}");
        }

        return url;
    }

 
    public Task IncrementClickAsync(string code)
    {
        _publisher.Publish(code);
        return Task.CompletedTask;
    }
}