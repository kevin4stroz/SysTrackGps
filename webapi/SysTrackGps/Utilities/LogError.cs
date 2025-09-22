using System;
using System.Data;
using System.Net;
using SysTrackGps.Application;
using SysTrackGps.Domain.Entities.Postgresql;
using SysTrackGps.Infraestructure.Data;
using SysTrackGps.Infraestructure.GenericRepository;

namespace SysTrackGps.Utilities;

/// <summary>
/// Clase de insercion de errores de la aplicacion
/// </summary>
public class LogError : ILogError
{
    private readonly IGenericRepository<ErrorLog> _errorLogGenericRepo;
    private readonly IDatabaseContext _databaseContext;

    public LogError(IGenericRepository<ErrorLog> errorLogGenericRepo, IDatabaseContext databaseContext)
    {
        _errorLogGenericRepo = errorLogGenericRepo;
        _databaseContext = databaseContext;
    }

    /// <summary>
    /// Metodo para insertar errores
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="controller_name"></param>
    /// <param name="method_name"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public async Task<ResponseDto<T>> InsertLog<T>(string controller_name, string method_name, Exception exception)
    {
        ErrorLog current_error_log = new ErrorLog()
        {
            id_error_log = Guid.NewGuid(),
            controller_name = controller_name,
            method_name = method_name,
            stack_trace = exception.StackTrace == null ? "No stacktrace available" : exception.StackTrace,
            code = GenCodeErrorResponse.BuildErrorCode(),
            created_date = DateTime.Now
        };

        IDbTransaction current_transaction = _databaseContext.BeginTransaction();
        await _errorLogGenericRepo.InsertAsync(current_transaction, current_error_log);
        _databaseContext.Commit();

        return new ResponseDto<T>()
        {
            status_code = (int)HttpStatusCode.InternalServerError,
            message = $"Ocurrio un error : Codigo {current_error_log.code}"
        };
    }
}
