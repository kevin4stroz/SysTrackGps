using System;
using System.Data;
using System.Net;
using SysTrackGps.Application;
using SysTrackGps.Application.Dtos;
using SysTrackGps.Domain.Entities.AlgoritmoA;
using SysTrackGps.Domain.Entities.Postgresql;
using SysTrackGps.Domain.Entities.RabbitMq;
using SysTrackGps.Domain.Entities.Redis;
using SysTrackGps.Infraestructure;
using SysTrackGps.Infraestructure.Data;
using SysTrackGps.Infraestructure.GenericRepository;
using SysTrackGps.Utilities;

namespace SysTrackGps.Services;

public class RutasService : IRutasService
{

    private readonly IRedisRepository _redisRepository;
    private readonly IVehiculosRepository _vehiculoRepository;
    private readonly IGenericRepository<VehiculoStatus> _vehiculoStatusGenericRepository;
    private readonly IGenericRepository<VehiculoViaje> _vehiculoViajeGenericRepository;
    private readonly IRutaRepository _rutaRepository;
    private readonly IRabbitMqProducer _rabbitMqProducer;

    public RutasService(
        IRedisRepository redisRepository,
        IVehiculosRepository vehiculoRepository,
        IGenericRepository<VehiculoStatus> vehiculoStatusGenericRepository,
        IGenericRepository<VehiculoViaje> vehiculoViajeGenericRepository,
        IRutaRepository rutaRepositoy,
        IRabbitMqProducer rabbitMqProducer)
    {
        _redisRepository = redisRepository;
        _vehiculoRepository = vehiculoRepository;
        _vehiculoStatusGenericRepository = vehiculoStatusGenericRepository;
        _vehiculoViajeGenericRepository = vehiculoViajeGenericRepository;
        _rutaRepository = rutaRepositoy;
        _rabbitMqProducer = rabbitMqProducer;
    }

    public async Task<ResponseDto<List<LocalidadDto>>> GetAllLocalidades()
    {
        List<LocalidadRedis> allLocalidades = (await _redisRepository.GetAllLocalidadesAsync()).ToList();

        if (allLocalidades.Count < 1)
        {
            return new ResponseDto<List<LocalidadDto>>()
            {
                message = "No existen localidades parametrizadas",
                result = [],
                status_code = (int)HttpStatusCode.InternalServerError
            };
        }

        return new ResponseDto<List<LocalidadDto>>()
        {
            message = "No existen localidades parametrizadas",
            result = allLocalidades.Select(x => new LocalidadDto() { Lat = x.Lat, Lon = x.Lon, Name = x.Name }).ToList(),
            status_code = (int)HttpStatusCode.OK
        };
    }

    public async Task<ResponseDto<ResponseIniciarViajeDto?>> IniciarViaje(IDbTransaction dbTransaction, IniciarViajeDto iniciarViajeDto)
    {
        // verificar que exista el vehiculo y que este disponible
        Vehiculo? current_available_vehiculo = await _vehiculoRepository.IsAvailableVehiculo(dbTransaction, iniciarViajeDto.id_vehiculo, StaticCodes.STATUS_DISPONIBLE, true);

        if (current_available_vehiculo == null)
        {
            return new ResponseDto<ResponseIniciarViajeDto?>()
            {
                message = "El vehiculo no se encuentra disponible para iniciar un nuevo viaje",
                result = null,
                status_code = (int)HttpStatusCode.Conflict
            };
        }

        // verificar que las localidades no sean la misma
        if (string.Equals(iniciarViajeDto.localidad_origen, iniciarViajeDto.localidad_destino))
        {
            return new ResponseDto<ResponseIniciarViajeDto?>()
            {
                message = "Las localidades no pueden ser las mismas",
                result = null,
                status_code = (int)HttpStatusCode.Conflict
            };
        }

        // verificar que exista localidad de origen y de destino
        List<LocalidadRedis> allLocalidades = (await _redisRepository.GetAllLocalidadesAsync()).ToList();

        LocalidadRedis? origen = allLocalidades.Where(x => string.Equals(x.Name, iniciarViajeDto.localidad_origen)).FirstOrDefault();
        LocalidadRedis? destino = allLocalidades.Where(x => string.Equals(x.Name, iniciarViajeDto.localidad_destino)).FirstOrDefault();

        if (origen == null || destino == null)
        {
            return new ResponseDto<ResponseIniciarViajeDto?>()
            {
                message = "Alguna de las dos localidades no existen",
                result = null,
                status_code = (int)HttpStatusCode.Conflict
            };
        }

        // calcular ruta mas corta entre las dos localidades
        AStarResult result = await _redisRepository.FindShortestPathAsync(iniciarViajeDto.localidad_origen, iniciarViajeDto.localidad_destino);

        if (result.Path.Count == 0)
        {
            return new ResponseDto<ResponseIniciarViajeDto?>()
            {
                message = "No se encontro ruta entre las localidades",
                result = null,
                status_code = (int)HttpStatusCode.Conflict
            };
        }

        // deshabilitar estado actual y insertar nuevo estado actual del vehiculo por en curso
        VehiculoStatus? status_en_curso = (await _vehiculoStatusGenericRepository.ListAsync(
            dbTransaction,
            "descripcion = @descripcion",
            new
            {
                descripcion = StaticCodes.STATUS_EN_CURSO
            })).FirstOrDefault();

        if (status_en_curso == null)
        {
            return new ResponseDto<ResponseIniciarViajeDto?>()
            {
                message = "Status de carro en curso no parametrizado",
                result = null,
                status_code = (int)HttpStatusCode.Conflict
            };
        }

        Guid new_id_veh_veh_status = await _vehiculoRepository.ChangeCurrentStatusVehiculo(dbTransaction, iniciarViajeDto.id_vehiculo, status_en_curso.id_vehiculo_status);

        // insertar nuevo viaje
        Guid id_vehiculo_viaje = Guid.NewGuid();
        VehiculoViaje new_viaje = new VehiculoViaje()
        {
            created_date = DateTime.Now,
            destino = iniciarViajeDto.localidad_destino,
            origen = iniciarViajeDto.localidad_origen,
            id_vehiculo_vehiculo_status = new_id_veh_veh_status,
            id_vehiculo_viaje = id_vehiculo_viaje,
            key_redis_ruta = $"Ruta-{current_available_vehiculo.placa}-{id_vehiculo_viaje}"
        };

        // insertar ruta de viaje en redis
        await _redisRepository.SetAsync(new_viaje.key_redis_ruta, result);

        // insertar viaje
        await _vehiculoViajeGenericRepository.InsertAsync(dbTransaction, new_viaje);

        // retornar id del viaje y ruta mas corta
        return new ResponseDto<ResponseIniciarViajeDto?>()
        {
            message = "Viaje iniciado, se adjunta ruta mas corta entre las dos localidades",
            status_code = (int)HttpStatusCode.OK,
            result = new ResponseIniciarViajeDto()
            {
                id_vehiculo_viaje = new_viaje.id_vehiculo_viaje,
                ruta_mas_corta = result
            }
        };
    }

    public async Task<ResponseDto<dynamic>> RecvCoordsCurrentPosition(RecvCoordsDto recvCoordsDto)
    {
        // obtener vehiculo_viaje actual si existe
        VehiculoViaje? current_viaje = await _rutaRepository.GetCurrentVehiculoViaje(recvCoordsDto.id_vehiculo);

        if (current_viaje == null)
        {
            return new ResponseDto<dynamic>()
            {
                message = "El vehiculo no se encuentra en un viaje actual",
                result = null,
                status_code = (int)HttpStatusCode.Conflict
            };
        }

        // encolar mensaje de procesado para rabbitmq
        _rabbitMqProducer.SendToProcessCurrentPosition<MessageToProcess>(new MessageToProcess()
        {
            id_vehiculo_viaje = current_viaje.id_vehiculo_viaje,
            id_vehiculo = recvCoordsDto.id_vehiculo,
            latitud = recvCoordsDto.latitud,
            longitud = recvCoordsDto.longitud
        });

        return new ResponseDto<dynamic>()
        {
            message = "Coordenadas procesadas de manera asincrona... mensaje queue",
            result = null,
            status_code = (int)HttpStatusCode.OK
        };

    }
}
