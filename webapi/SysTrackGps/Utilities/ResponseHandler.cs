using System;
using Microsoft.AspNetCore.Mvc;
using SysTrackGps.Application;

namespace SysTrackGps.Utilities;

public class ResponseHandler
{
    public static ActionResult<ResponseDto<T>> HandleResponse<T>(ResponseDto<T> response)
    {
        return response.status_code switch
        {
            200 => new OkObjectResult(response),
            400 => new BadRequestObjectResult(response),
            401 => new UnauthorizedObjectResult(response),
            403 => new ForbidResult(),
            404 => new NotFoundObjectResult(response),
            500 => new ObjectResult(response) { StatusCode = 500 },
            _ => new ObjectResult(response) { StatusCode = response.status_code }
        };
    }

}
