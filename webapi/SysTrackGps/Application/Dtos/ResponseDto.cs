using System;

namespace SysTrackGps.Application;

public class ResponseDto<T>
{
    public int status_code { get; set; }
    public string? message { get; set; }
    public T? result { get; set; }
}
