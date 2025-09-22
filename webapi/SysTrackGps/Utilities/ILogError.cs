using System;
using SysTrackGps.Application;

namespace SysTrackGps.Utilities;

public interface ILogError
{
    Task<ResponseDto<T>> InsertLog<T>(string controller_name, string method_name, Exception exception);
}
