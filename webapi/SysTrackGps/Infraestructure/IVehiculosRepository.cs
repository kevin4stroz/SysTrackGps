using System;
using System.Data;
using SysTrackGps.Domain.Entities.Postgresql;

namespace SysTrackGps.Infraestructure;

public interface IVehiculosRepository
{
    Task<Vehiculo?> IsAvailableVehiculo(IDbTransaction dbTransaction, Guid id_vehiculo, string descripcion, bool flg_current_status);
    Task<Guid> ChangeCurrentStatusVehiculo(IDbTransaction dbTransaction, Guid id_vehiculo, Guid id_vehiculo_status);
}
