using Microsoft.AspNetCore.Mvc;
using SysTrackGps.Application;
using SysTrackGps.Application.Dtos;
using SysTrackGps.Domain.Entities.Postgresql;
using SysTrackGps.Utilities;

namespace SysTrackGps.Controllers
{
    /// <summary>
    /// Controlador de vehiculos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class VehiculosController : ControllerBase
    {

        private readonly IVehiculosApplication _vehiculosApplication;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vehiculosApplication"></param>
        public VehiculosController(IVehiculosApplication vehiculosApplication)
        {
            _vehiculosApplication = vehiculosApplication;
        }

        /// <summary>
        /// Lista datos maestros de estado de vehiculo
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetVehiculoStatusList")]
        public async Task<ActionResult<ResponseDto<List<VehiculoStatus>>>> GetVehiculoStatusList()
        {
            ActionResult<ResponseDto<List<VehiculoStatus>>> response = ResponseHandler.HandleResponse(
                await _vehiculosApplication.GetVehiculoStatusList());
            return response;
        }

        /// <summary>
        /// Creacion de vehiculo
        /// </summary>
        /// <param name="vehiculoCreateDto"></param>
        /// <returns></returns>
        [HttpPost("CreateVehiculo")]
        public async Task<ActionResult<ResponseDto<ResponseCreateVehiculo?>>> CreateVehiculo(VehiculoCreateDto vehiculoCreateDto)
        {
            ActionResult<ResponseDto<ResponseCreateVehiculo?>> response = ResponseHandler.HandleResponse(
                await _vehiculosApplication.CreateVehiculo(vehiculoCreateDto));
            return response;
        }
    }
}
