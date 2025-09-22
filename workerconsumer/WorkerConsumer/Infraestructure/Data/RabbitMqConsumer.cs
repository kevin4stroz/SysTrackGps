using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WorkerConsumer.Infraestructure.Data;

public class RabbitMqConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IConfiguration _config;
    private string QueueName = "";

    public RabbitMqConsumer(IConfiguration config)
    {
        _config = config;
        QueueName = _config.GetSection("RabbitMq:QueueName").Value!;

        var factory = new ConnectionFactory()
        {
            HostName = _config.GetSection("RabbitMq:HostName").Value,
            UserName = _config.GetSection("RabbitMq:UserName").Value,
            Password = _config.GetSection("RabbitMq:Password").Value,
            Uri = new Uri(_config.GetSection("RabbitMq:Url").Value!),
        };

        factory.Ssl.Enabled = false;

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
    }

    public void Consume(Func<string, Task> onMessage)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (ch, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await onMessage(message);

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
    }
}
