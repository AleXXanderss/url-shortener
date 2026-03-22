using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using UrlShortener.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace UrlShortener.Api.Services;

public class ClickQueueWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IModel? _channel;

    public ClickQueueWorker(IServiceScopeFactory scopeFactory, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _config = config;
    }

    private void ConnectWithRetry()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _config["RabbitMQ:Host"] ?? "rabbitmq"
        };

        while (true)
        {
            try
            {
                Console.WriteLine("Trying to connect to RabbitMQ...");

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: "clicks",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                Console.WriteLine("Connected to RabbitMQ");
                break;
            }
            catch
            {
                Console.WriteLine("RabbitMQ not ready, retrying...");
                Thread.Sleep(2000);
            }
        }
    }

    private void EnsureConnection()
    {
        if (_connection != null && _connection.IsOpen) return;
        ConnectWithRetry();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureConnection();

        var consumer = new EventingBasicConsumer(_channel!);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var shortCode = Encoding.UTF8.GetString(body);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var url = await db.Links.FirstOrDefaultAsync(l => l.ShortCode == shortCode);

            if (url != null)
            {
                url.Clicks++;
                await db.SaveChangesAsync();
            }

       
            _channel!.BasicAck(ea.DeliveryTag, false);
        };

        _channel!.BasicConsume(
            queue: "clicks",
            autoAck: false, 
            consumer: consumer);

        return Task.CompletedTask;
    }
}