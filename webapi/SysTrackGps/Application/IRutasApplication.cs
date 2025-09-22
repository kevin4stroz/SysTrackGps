using System;
using SysTrackGps.Application.Dtos;

namespace SysTrackGps.Application;

public interface IRutasApplication
{
    Task<ResponseDto<List<LocalidadDto>>> GetAllLocalidades();
    Task<ResponseDto<ResponseIniciarViajeDto?>> IniciarViaje(IniciarViajeDto iniciarViajeDto);
    Task<ResponseDto<dynamic>> RecvCoordsCurrentPosition(RecvCoordsDto recvCoordsDto);
}
