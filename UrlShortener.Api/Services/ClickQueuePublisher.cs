using RabbitMQ.Client;
using System.Text;

namespace UrlShortener.Api.Services;

public class ClickQueuePublisher
{
    private IConnection _connection;
    private IModel _channel;

    public ClickQueuePublisher(IConfiguration config)
    {
        var host = config["RabbitMQ:Host"] ?? "rabbitmq";
        ConnectWithRetry(host);
    }

    private void ConnectWithRetry(string host)
    {
        var factory = new ConnectionFactory()
        {
            HostName = host
        };

        while (true)
        {
            try
            {
                Console.WriteLine("Publisher connecting to RabbitMQ...");

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: "clicks",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                Console.WriteLine("Publisher connected");
                break;
            }
            catch
            {
                Console.WriteLine("RabbitMQ not ready (publisher), retrying...");
                Thread.Sleep(2000);
            }
        }
    }

    public void Publish(string shortCode)
    {
        var body = Encoding.UTF8.GetBytes(shortCode);

        _channel.BasicPublish(
            exchange: "",
            routingKey: "clicks",
            basicProperties: null,
            body: body
        );
    }
}