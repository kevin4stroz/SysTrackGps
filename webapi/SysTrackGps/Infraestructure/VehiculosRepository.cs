using System;
using System.Data;
using Dapper;
using SysTrackGps.Domain.Entities.Postgresql;
using SysTrackGps.Infraestructure.Data;
using SysTrackGps.Infraestructure.GenericRepository;

namespace SysTrackGps.Infraestructure;

public class VehiculosRepository : IVehiculosRepository
{
    private readonly IDatabaseContext _dbContext;
    private readonly IGenericRepository<VehiculoVehiculoStatus> _vehVehStatusGenericRepository;

    public VehiculosRepository(IDatabaseContext dbContext, IGenericRepository<VehiculoVehiculoStatus> vehVehStatusGenericRepository)
    {
        _dbContext = dbContext;
        _vehVehStatusGenericRepository = vehVehStatusGenericRepository;
    }

    public async Task<Vehiculo?> IsAvailableVehiculo(IDbTransaction dbTransaction, Guid id_vehiculo, string descripcion, bool flg_current_status)
    {
        string query = $@"
        SELECT 
            v.* 
        FROM 
            public.vehiculo as v
        INNER JOIN 
            public.vehiculo_vehiculo_status as vvs 
            on v.id_vehiculo = vvs.id_vehiculo
        INNER JOIN 
            public.vehiculo_status as vs 
            on vs.id_vehiculo_status = vvs.id_vehiculo_status
        WHERE 
            v.id_vehiculo = @id_vehiculo AND
            vs.descripcion = @descripcion AND
            vvs.flg_current_status = @flg_current_status
        ";

        var params_query = new { id_vehiculo, descripcion, flg_current_status };

        IEnumerable<Vehiculo> current_available_vehiculo = await dbTransaction.Connection!.QueryAsync<Vehiculo>(
            query, params_query, dbTransaction, null, dbTransaction.Connection!.CreateCommand().CommandType);

        return current_available_vehiculo.FirstOrDefault();
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
