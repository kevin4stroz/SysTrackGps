using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using SysTrackGps.Application;
using SysTrackGps.Infraestructure;
using SysTrackGps.Infraestructure.Data;
using SysTrackGps.Infraestructure.GenericRepository;
using SysTrackGps.Services;
using SysTrackGps.Utilities;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgresqlConnection")
    ?? throw new InvalidOperationException("Connection string 'PostgresqlConnection' not found.");

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' not found.");


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


builder.Services.AddScoped<IVehiculosApplication, VehiculosApplication>();
builder.Services.AddScoped<IRutasApplication, RutasApplication>();

builder.Services.AddScoped<IVehiculosService, VehiculosService>();
builder.Services.AddScoped<IRutasService, RutasService>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IRedisRepository, RedisRepository>();
builder.Services.AddScoped<IRabbitMqProducer, RabbitMqProducer>();
builder.Services.AddScoped<IVehiculosRepository, VehiculosRepository>();
builder.Services.AddScoped<IRutaRepository, RutaRepository>();
builder.Services.AddScoped<ILogError, LogError>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();
app.Run();


