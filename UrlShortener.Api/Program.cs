using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Services
builder.Services.AddScoped<ShortCodeService>();
builder.Services.AddSingleton<ClickQueuePublisher>();
builder.Services.AddHostedService<ClickQueueWorker>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var retries = 30;

    while (retries > 0)
    {
        try
        {
            Console.WriteLine("Trying to migrate DB...");
            db.Database.Migrate();
            Console.WriteLine("DB READY");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB not ready: {ex.Message}");
            retries--;
            Thread.Sleep(2000);
        }
    }

    if (retries == 0)
        throw new Exception("Database never became ready");
}

app.MapControllers();

app.Run();