using System;
using System.Data;
using SysTrackGps.Application;
using SysTrackGps.Application.Dtos;
using SysTrackGps.Domain.Entities.Postgresql;

namespace SysTrackGps.Services;

public interface IVehiculosService
{
    Task<ResponseDto<List<VehiculoStatus>>> GetVehiculoStatusList();
    Task<ResponseDto<ResponseCreateVehiculo?>> CreateVehiculo(IDbTransaction dbTransaction, Vehiculo vehiculoCreateDto);
}
