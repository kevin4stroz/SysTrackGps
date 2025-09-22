using System;
using System.Data;
using System.Text.Json;
using WorkerConsumer.Infraestructure;
using WorkerConsumer.Infraestructure.Data;
using WorkerConsumer.Infraestructure.Entities;
using WorkerConsumer.Service.Dtos;

namespace WorkerConsumer.Service;

public class MessageProcessor : IMessageProcessor
{
    private readonly ILogger<MessageProcessor> _logger;
    private readonly IGenericRepository<Vehiculo> _vehiculoGenericRepository;
    private readonly IGenericRepository<VehiculoViaje> _vehiculoViajeGenericRepository;
    private readonly IGenericRepository<PosicionActualViaje> _posActualViajeGenericRepository;
    private readonly IDatabaseContext _databaseContext;
    private readonly IRedisRepository _redisRepository;
    private readonly IVehiculoRepository _vehiculoRepository;

    public MessageProcessor(
        ILogger<MessageProcessor> logger,
        IGenericRepository<Vehiculo> vehiculoGenericRepository,
        IGenericRepository<VehiculoViaje> vehiculoViajeGenericRepository,
        IGenericRepository<PosicionActualViaje> posActualViajeGenericRepository,
        IDatabaseContext databaseContext,
        IRedisRepository redisRepository,
        IVehiculoRepository vehiculoRepository)
    {
        _logger = logger;
        _vehiculoGenericRepository = vehiculoGenericRepository;
        _vehiculoViajeGenericRepository = vehiculoViajeGenericRepository;
        _posActualViajeGenericRepository = posActualViajeGenericRepository;
        _databaseContext = databaseContext;
        _redisRepository = redisRepository;
        _vehiculoRepository = vehiculoRepository;

    }

    public async Task ProcessMessageAsync(string message)
    {
        MessageToProcess? current_message;

        try
        {
            current_message = JsonSerializer.Deserialize<MessageToProcess>(message);
        }
        catch (JsonException jex)
        {
            _logger.LogError(jex, "Error al deserializar mensaje: {Message}", message);
            return;
        }

        if (current_message == null)
        {
            _logger.LogError("Error al deserializar mensaje: {Message}", message);
            return;
        }

        _logger.LogInformation("Procesando mensaje {Id}", current_message.id_vehiculo_viaje);

        IDbTransaction transaction = _databaseContext.BeginTransaction();

        try
        {
            // Obtener vehiculo actual
            Vehiculo? current_vehiculo = (await _vehiculoGenericRepository.ListAsync(
                transaction,
                "id_vehiculo = @id_vehiculo",
                new { id_vehiculo = current_message.id_vehiculo }
            )).Single();

            // Obtener vehiculo viaje actual
            VehiculoViaje? current_vehiculo_viaje = (await _vehiculoViajeGenericRepository.ListAsync(
                transaction,
                "id_vehiculo_viaje = @id_vehiculo_viaje",
                new { id_vehiculo_viaje = current_message.id_vehiculo_viaje }
            )).Single();

            // Insertar entidad PosicionActualViaje
            PosicionActualViaje new_posicion = new PosicionActualViaje()
            {
                created_date = DateTime.Now,
                id_posicion_actual_viaje = Guid.NewGuid(),
                id_vehiculo_viaje = current_vehiculo_viaje.id_vehiculo_viaje,
                latitud = current_message.latitud,
                longitud = current_message.longitud
            };

            
            await _posActualViajeGenericRepository.InsertAsync(transaction, new_posicion);


            // Verificar si la coordenada insertada esta demasiado cerca del destino para cerrar el proceso
            LocalidadRedis? current_destino = await _redisRepository.GetLocalidadByNameAsync(current_vehiculo_viaje.destino);

            if (current_destino == null)
            {
                _logger.LogError($"Destino no existe : {current_vehiculo_viaje.destino}");
                return;
            }

            double distancia = current_destino.DistanciaHasta(current_message.latitud, current_message.longitud);

            if (distancia <= 20)
            {
                _logger.LogInformation("Vehiculo llego a su destino");

                // cambiar de estado a DISPONIBLE
                await _vehiculoRepository.ChangeCurrentStatusVehiculo(transaction, current_vehiculo.id_vehiculo, Guid.Parse("9c017058-bf55-4919-bade-357fa72b5612"));
            }

            // realizar commit
            _databaseContext.Commit();

            // Insertar Coordenadas en lista de redis
            Coords coords = new Coords()
            {
                latitud = current_message.latitud,
                longitud = current_message.longitud

            };

            await _redisRepository.AddToListAsync<Coords>($"COORDS-{current_vehiculo_viaje.key_redis_ruta}", coords);

            return;
        }
        catch (Exception ex) {
            _logger.LogError("Excepcion en la persistencia del mensaje : {StackTrace}", ex.StackTrace);
            _databaseContext.RollBack();
            return;
        }       

    }
}
