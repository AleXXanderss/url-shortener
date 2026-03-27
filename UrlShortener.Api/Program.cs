using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Api.Data;
using UrlShortener.Api.Services;
using UrlShortener.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

// Services
builder.Services.AddScoped<ShortCodeService>();
builder.Services.AddScoped<RedirectService>();
builder.Services.AddSingleton<ClickQueuePublisher>();
builder.Services.AddHostedService<ClickQueueWorker>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// DB Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    for (int i = 0; i < 10; i++)
    {
        try
        {
            Console.WriteLine("Applying migrations...");
            db.Database.Migrate();
            Console.WriteLine("DB ready");
            break;
        }
        catch
        {
            Console.WriteLine("DB not ready, retry...");
            Thread.Sleep(2000);
        }
    }
}

// Routes
app.MapControllers();

app.Run();