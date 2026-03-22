using RabbitMQ.Client;
using System.Text;

namespace UrlShortener.Api.Services
{
    public class ClickQueuePublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public ClickQueuePublisher()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq"
            };

            // retry на случай, если RabbitMQ еще не поднялся
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Console.WriteLine("Connecting to RabbitMQ...");

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.QueueDeclare(
                        queue: "clicks",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    Console.WriteLine("Connected to RabbitMQ");
                    return;
                }
                catch
                {
                    Console.WriteLine("RabbitMQ not ready, retrying...");
                    Thread.Sleep(2000);
                }
            }

            throw new Exception("Failed to connect to RabbitMQ");
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

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}