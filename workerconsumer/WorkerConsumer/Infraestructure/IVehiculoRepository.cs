using System;
using System.Data;

namespace WorkerConsumer.Infraestructure;

public interface IVehiculoRepository
{
    Task<Guid> ChangeCurrentStatusVehiculo(IDbTransaction dbTransaction, Guid id_vehiculo, Guid id_vehiculo_status);
}
