using System;
using System.Data;
using Dapper;
using WorkerConsumer.Infraestructure.Data;
using WorkerConsumer.Infraestructure.Entities;

namespace WorkerConsumer.Infraestructure;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly IGenericRepository<VehiculoVehiculoStatus> _vehVehStatusGenericRepository;

    public VehiculoRepository(IGenericRepository<VehiculoVehiculoStatus> vehVehStatusGenericRepository)
    {
        _vehVehStatusGenericRepository = vehVehStatusGenericRepository;
    }

    public async Task<Guid> ChangeCurrentStatusVehiculo(IDbTransaction dbTransaction, Guid id_vehiculo, Guid id_vehiculo_status)
    {
        string query = $@"
            UPDATE 
                public.vehiculo_vehiculo_status
            SET 
                flg_current_status = false
            WHERE id_vehiculo = @id_vehiculo;
        ";

        await dbTransaction.Connection!.QueryAsync(query, new { id_vehiculo }, dbTransaction, null, dbTransaction.Connection!.CreateCommand().CommandType);

        VehiculoVehiculoStatus new_status = new VehiculoVehiculoStatus()
        {
            created_date = DateTime.Now,
            flg_current_status = true,
            id_vehiculo = id_vehiculo,
            id_vehiculo_status = id_vehiculo_status,
            id_vehiculo_vehiculo_status = Guid.NewGuid()
        };

        await _vehVehStatusGenericRepository.InsertAsync(dbTransaction, new_status);

        return new_status.id_vehiculo_vehiculo_status;
    }
}
