using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using SysTrackGps.Application.Dtos;
using SysTrackGps.Infraestructure.Data;
using SysTrackGps.Services;
using SysTrackGps.Utilities;

namespace SysTrackGps.Application;

public class RutasApplication : IRutasApplication
{
    private readonly IRutasService _rutasService;
    private readonly ILogError _logError;
    private readonly IDatabaseContext _databaseContext;

    private const string CONTROLLER_NAME = "RutasController";


    public RutasApplication(ILogError logError, IRutasService rutasService, IDatabaseContext databaseContext)
    {
        _logError = logError;
        _rutasService = rutasService;
        _databaseContext = databaseContext;
    }

    public async Task<ResponseDto<List<LocalidadDto>>> GetAllLocalidades()
    {
        MethodBase? method = await this.GetMethodInfo(new StackTrace());

        try
        {

            var response = await _rutasService.GetAllLocalidades();

            return response;
        }
        catch (Exception ex)
        {
            return await _logError.InsertLog<List<LocalidadDto>>(CONTROLLER_NAME, method!.Name, ex);
        }
    }

    public async Task<ResponseDto<ResponseIniciarViajeDto?>> IniciarViaje(IniciarViajeDto iniciarViajeDto)
    {
        MethodBase? method = await this.GetMethodInfo(new StackTrace());

        try
        {
            IDbTransaction dbTransaction = _databaseContext.BeginTransaction();
            var response = await _rutasService.IniciarViaje(dbTransaction, iniciarViajeDto);
            _databaseContext.Commit();

            return response;
        }
        catch (Exception ex)
        {
            _databaseContext.RollBack();
            return await _logError.InsertLog<ResponseIniciarViajeDto?>(CONTROLLER_NAME, method!.Name, ex);
        }
    }


    public async Task<ResponseDto<dynamic>> RecvCoordsCurrentPosition(RecvCoordsDto recvCoordsDto)
    {
        MethodBase? method = await this.GetMethodInfo(new StackTrace());

        try
        {
            var response = await _rutasService.RecvCoordsCurrentPosition(recvCoordsDto);

            return response;
        }
        catch (Exception ex)
        {
            return await _logError.InsertLog<dynamic>(CONTROLLER_NAME, method!.Name, ex);
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
