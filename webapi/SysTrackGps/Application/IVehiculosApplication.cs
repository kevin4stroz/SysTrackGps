using System;
using Microsoft.AspNetCore.Mvc;
using SysTrackGps.Application.Dtos;
using SysTrackGps.Domain.Entities.Postgresql;

namespace SysTrackGps.Application;

public interface IVehiculosApplication
{
    Task<ResponseDto<List<VehiculoStatus>>> GetVehiculoStatusList();
    Task<ResponseDto<ResponseCreateVehiculo?>> CreateVehiculo(VehiculoCreateDto vehiculoCreateDto);
}
