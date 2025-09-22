using StackExchange.Redis;
using WorkerConsumer;
using WorkerConsumer.Infraestructure;
using WorkerConsumer.Infraestructure.Data;
using WorkerConsumer.Infraestructure.Entities;
using WorkerConsumer.Service;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresqlConnection")
    ?? throw new InvalidOperationException("Connection string 'PostgresqlConnection' not found.");

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' not found.");

builder.Services.AddSingleton<RabbitMqConsumer>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddStackExchangeRedisCache(
    options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "SysTrackGps";
    }
);

builder.Services.AddScoped<IDatabaseContext>(_ => new DatabaseContext(connectionString));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IMessageProcessor, MessageProcessor>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();

var host = builder.Build();
host.Run();
