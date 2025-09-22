using WorkerConsumer.Infraestructure.Data;
using WorkerConsumer.Service;

namespace WorkerConsumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqConsumer _consumer;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, RabbitMqConsumer consumer, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _consumer = consumer;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Consume(async message =>
        {
            _logger.LogInformation("Mensaje recibido: {Message}", message);

            // Crear scope para cada mensaje
            using var scope = _serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IMessageProcessor>();

            try
            {
                await processor.ProcessMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje: {Message}", message);
            }
        });

        return Task.CompletedTask;
    }
}
