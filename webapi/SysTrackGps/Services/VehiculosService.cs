using System;
using System.Data;
using System.Net;
using SysTrackGps.Application;
using SysTrackGps.Application.Dtos;
using SysTrackGps.Domain.Entities.Postgresql;
using SysTrackGps.Infraestructure.GenericRepository;
using SysTrackGps.Utilities;

namespace SysTrackGps.Services;

public class VehiculosService : IVehiculosService
{

    private readonly IGenericRepository<VehiculoStatus> _vehiculoStatusGenericRepo;
    private readonly IGenericRepository<Vehiculo> _vehiculoGenericRepo;
    private readonly IGenericRepository<VehiculoVehiculoStatus> _vehVehStatusGenericRepo;


    public VehiculosService(
        IGenericRepository<VehiculoStatus> vehiculoStatusGenericRepo,
        IGenericRepository<Vehiculo> vehiculoGenericRepo,
        IGenericRepository<VehiculoVehiculoStatus> vehVehStatusGenericRepo
    )
    {
        _vehiculoStatusGenericRepo = vehiculoStatusGenericRepo;
        _vehiculoGenericRepo = vehiculoGenericRepo;
        _vehVehStatusGenericRepo = vehVehStatusGenericRepo;
    }

    public async Task<ResponseDto<List<VehiculoStatus>>> GetVehiculoStatusList()
    {
        IEnumerable<VehiculoStatus> listStatus = await _vehiculoStatusGenericRepo.ListAsync();

        return new ResponseDto<List<VehiculoStatus>>()
        {
            status_code = (int)HttpStatusCode.OK,
            message = "Listado de estados parametrizados para vehiculos",
            result = listStatus.ToList()
        };
    }

    public async Task<ResponseDto<ResponseCreateVehiculo?>> CreateVehiculo(IDbTransaction dbTransaction, Vehiculo vehiculoCreate)
    {
        // Verificar si ya existe un vehiculo con la placa
        IEnumerable<Vehiculo> current_vehiculo = await _vehiculoGenericRepo.ListAsync(
            dbTransaction,
            "placa = @placa",
            new { placa = vehiculoCreate.placa }
        );

        if (current_vehiculo.Any())
        {
            return new ResponseDto<ResponseCreateVehiculo?>()
            {
                message = "Ya existe un vehiculo registrado con esa placa",
                result = null,
                status_code = (int)HttpStatusCode.Conflict
            };
        }

        // Insertar nuevo vehiculo
        await _vehiculoGenericRepo.InsertAsync(dbTransaction, vehiculoCreate);

        // Insertar status por default en DISPONIBLE
        VehiculoStatus? default_status = (await _vehiculoStatusGenericRepo.ListAsync(
            dbTransaction,
            "descripcion = @descripcion",
            new { descripcion = StaticCodes.STATUS_DISPONIBLE }
        )).FirstOrDefault();

        if (default_status == null)
        {
            return new ResponseDto<ResponseCreateVehiculo?>()
            {
                message = "Status de carro disponible no parametrizado",
                result = null,
                status_code = (int)HttpStatusCode.InternalServerError
            };
        }

        VehiculoVehiculoStatus new_vehiculo_status = new VehiculoVehiculoStatus()
        {
            created_date = DateTime.Now,
            flg_current_status = true,
            id_vehiculo = vehiculoCreate.id_vehiculo,
            id_vehiculo_status = default_status.id_vehiculo_status,
            id_vehiculo_vehiculo_status = Guid.NewGuid()
        };

        await _vehVehStatusGenericRepo.InsertAsync(dbTransaction, new_vehiculo_status);

        // retornar valor        
        return new ResponseDto<ResponseCreateVehiculo?>()
        {
            message = "Vehiculo creado exitosamente",
            result = new ResponseCreateVehiculo()
            {
                created_date = vehiculoCreate.created_date,
                id_vehiculo = vehiculoCreate.id_vehiculo
            },
            status_code = (int)HttpStatusCode.Accepted
        };        
    }
}
