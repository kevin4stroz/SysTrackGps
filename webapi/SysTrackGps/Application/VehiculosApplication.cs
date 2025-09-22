using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using SysTrackGps.Application.Dtos;
using SysTrackGps.Domain.Entities.Postgresql;
using SysTrackGps.Infraestructure.Data;
using SysTrackGps.Services;
using SysTrackGps.Utilities;

namespace SysTrackGps.Application;

public class VehiculosApplication : IVehiculosApplication
{

    private readonly IVehiculosService _vehiculosService;
    private readonly IDatabaseContext _databaseContext;
    private readonly ILogError _logError;
    private const string CONTROLLER_NAME = "VehiculosController";

    /// <summary>
    /// Contructor
    /// </summary>
    /// <param name="vehiculosService"></param>
    /// <param name="logError"></param>
    public VehiculosApplication(IVehiculosService vehiculosService, ILogError logError, IDatabaseContext databaseContext)
    {
        _vehiculosService = vehiculosService;
        _logError = logError;
        _databaseContext = databaseContext;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<ResponseDto<List<VehiculoStatus>>> GetVehiculoStatusList()
    {
        MethodBase? method = await this.GetMethodInfo(new StackTrace());

        try
        {
            return await _vehiculosService.GetVehiculoStatusList();
        }
        catch (Exception ex)
        {
            return await _logError.InsertLog<List<VehiculoStatus>>(CONTROLLER_NAME, method!.Name, ex);
        }
    }


    public async Task<ResponseDto<ResponseCreateVehiculo?>> CreateVehiculo(VehiculoCreateDto vehiculoCreateDto)
    {
        MethodBase? method = await this.GetMethodInfo(new StackTrace());

        try
        {
            IDbTransaction dbTransaction = _databaseContext.BeginTransaction();

            Vehiculo new_vehiculo = new Vehiculo()
            {
                id_vehiculo = Guid.NewGuid(),
                capacidad_carga = vehiculoCreateDto.capacidad_carga,
                capacidad_pasajeros = vehiculoCreateDto.capacidad_pasajeros,
                cilindraje = vehiculoCreateDto.cilindraje,
                color = vehiculoCreateDto.color,
                created_date = DateTime.Now,
                modelo = vehiculoCreateDto.modelo,
                placa = vehiculoCreateDto.placa
            };

            var response = await _vehiculosService.CreateVehiculo(dbTransaction, new_vehiculo);
            _databaseContext.Commit();

            return response;
        }
        catch (Exception ex)
        {
            _databaseContext.RollBack();
            return await _logError.InsertLog<ResponseCreateVehiculo?>(CONTROLLER_NAME, method!.Name, ex);
        }
    }
    

    /// <summary>
    /// GetMethodInfo : Obtiene informacion del metodo de la invocacion
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    private async Task<MethodBase?> GetMethodInfo(StackTrace method)
    {
        var methodInfo = method.GetFrames().Select(frame => frame.GetMethod()).FirstOrDefault(item => item?.DeclaringType == GetType());
        await Task.Yield();
        return methodInfo;
    }
    

}
