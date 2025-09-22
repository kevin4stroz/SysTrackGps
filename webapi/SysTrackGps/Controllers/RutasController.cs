using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SysTrackGps.Application;
using SysTrackGps.Application.Dtos;
using SysTrackGps.Utilities;

namespace SysTrackGps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RutasController : ControllerBase
    {

        private readonly IRutasApplication _rutasApplication;

        public RutasController(IRutasApplication rutasApplication)
        {
            _rutasApplication = rutasApplication;
        }

        /// <summary>
        /// Obtener todas las rutas parametrizadas en el sistema
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllLocalidades")]
        public async Task<ActionResult<ResponseDto<List<LocalidadDto>>>> GetAllLocalidades()
        {
            ActionResult<ResponseDto<List<LocalidadDto>>> response = ResponseHandler.HandleResponse(
                await _rutasApplication.GetAllLocalidades());

            return response;
        }

        /// <summary>
        /// Iniciar ruta de un vehiculo disponible, localidad de origen y localidad de destino
        /// </summary>
        /// <param name="iniciarViajeDto"></param>
        /// <returns></returns>
        [HttpPost("IniciarViaje")]
        public async Task<ActionResult<ResponseDto<ResponseIniciarViajeDto?>>> IniciarViaje(IniciarViajeDto iniciarViajeDto)
        {
            ActionResult<ResponseDto<ResponseIniciarViajeDto?>> response = ResponseHandler.HandleResponse(
                await _rutasApplication.IniciarViaje(iniciarViajeDto));

            return response;

        }
        
        /// <summary>
        /// Recibe las coordenadas de un viaje actual y las encola a rabbitmq
        /// </summary>
        /// <param name="recvCoordsDto"></param>
        /// <returns></returns>
        [HttpPost("RecvCoordsCurrentPosition")]
        public async Task<ActionResult<ResponseDto<dynamic>>> RecvCoordsCurrentPosition(RecvCoordsDto recvCoordsDto)
        {
            ActionResult<ResponseDto<dynamic>> response = ResponseHandler.HandleResponse(
                await _rutasApplication.RecvCoordsCurrentPosition(recvCoordsDto));

            return response;
        }
        
    }
}
