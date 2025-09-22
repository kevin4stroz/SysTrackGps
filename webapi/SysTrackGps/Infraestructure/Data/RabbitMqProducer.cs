using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SysTrackGps.Domain.Entities.RabbitMq;

namespace SysTrackGps.Infraestructure.Data;

public class RabbitMqProducer : IRabbitMqProducer
{

    private RabbitMqConfiguration _rabbitMqConfiguration;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel? _channelSendToProcessCurrentPosition;

    public RabbitMqProducer(IConfiguration config)
    {
        _config = config;

        _rabbitMqConfiguration = new RabbitMqConfiguration()
        {
            UserName = _config.GetSection("RabbitMq:UserName").Value,
            Password = _config.GetSection("RabbitMq:Password").Value,
            VirtualHost = _config.GetSection("RabbitMq:VirtualHost").Value,
            HostName = _config.GetSection("RabbitMq:HostName").Value,
            Port = _config.GetSection("RabbitMq:Port").Value,
            Url = _config.GetSection("RabbitMq:Url").Value,
            QueueName = _config.GetSection("RabbitMq:QueueName").Value,
            ExchangeName = _config.GetSection("RabbitMq:ExchangeName").Value,
        };
    }

    private void InitConn()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqConfiguration.HostName,
                UserName = _rabbitMqConfiguration.UserName,
                Password = _rabbitMqConfiguration.Password,
                Uri = new Uri(_rabbitMqConfiguration.Url!),
            };
            factory.Ssl.Enabled = false;

            // create connection
            _connection = factory.CreateConnection();

            // create _channel_code_consecutive_crediflores
            if (_channelSendToProcessCurrentPosition == null || !_channelSendToProcessCurrentPosition.IsOpen)
            {
                _channelSendToProcessCurrentPosition = _connection.CreateModel();
                _channelSendToProcessCurrentPosition.QueueDeclare("SendToProcessCurrentPosition", true, false, false, null);
            }
        }
    }

    public void SendToProcessCurrentPosition<T>(T message)
    {
        InitConn();
        using (_connection)
        {
            using (_channelSendToProcessCurrentPosition)
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);
                _channelSendToProcessCurrentPosition!.BasicPublish(exchange: "", routingKey: "SendToProcessCurrentPosition", body: body);
                _channelSendToProcessCurrentPosition!.Close();
                _connection!.Close();
            }
        }
    }
}
