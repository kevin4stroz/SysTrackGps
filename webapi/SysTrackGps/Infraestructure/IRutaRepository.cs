using System;
using SysTrackGps.Domain.Entities.Postgresql;

namespace SysTrackGps.Infraestructure;

public interface IRutaRepository
{
    Task<VehiculoViaje?> GetCurrentVehiculoViaje(Guid id_vehiculo);
}
