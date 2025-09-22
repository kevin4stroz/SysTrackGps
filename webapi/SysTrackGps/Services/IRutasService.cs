using System;
using System.Data;
using SysTrackGps.Application;
using SysTrackGps.Application.Dtos;

namespace SysTrackGps.Services;

public interface IRutasService
{
    Task<ResponseDto<List<LocalidadDto>>> GetAllLocalidades();
    Task<ResponseDto<ResponseIniciarViajeDto?>> IniciarViaje(IDbTransaction dbTransaction, IniciarViajeDto iniciarViajeDto);
    Task<ResponseDto<dynamic>> RecvCoordsCurrentPosition(RecvCoordsDto recvCoordsDto);
}
