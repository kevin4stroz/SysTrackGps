using System;
using Dapper;
using SysTrackGps.Domain.Entities.Postgresql;
using SysTrackGps.Infraestructure.Data;
using SysTrackGps.Utilities;

namespace SysTrackGps.Infraestructure;

public class RutaRepository : IRutaRepository
{
    private readonly IDatabaseContext _databaseContext;

    public RutaRepository(IDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }


    public async Task<VehiculoViaje?> GetCurrentVehiculoViaje(Guid id_vehiculo)
    {
        string query = $@"
            SELECT 
                vviaje.* 
            FROM 
                public.vehiculo as v
            INNER JOIN 
                public.vehiculo_vehiculo_status as vvs 
                on v.id_vehiculo = vvs.id_vehiculo
            INNER JOIN 
                public.vehiculo_status as vs 
                on vs.id_vehiculo_status = vvs.id_vehiculo_status
            INNER JOIN
                public.vehiculo_viaje as vviaje
                on vviaje.id_vehiculo_vehiculo_status = vvs.id_vehiculo_vehiculo_status
            WHERE 
                v.id_vehiculo = @id_vehiculo AND
                vs.descripcion = @descripcion AND
                vvs.flg_current_status = @flg_current_status
        ";

        var params_query = new { id_vehiculo, descripcion = StaticCodes.STATUS_EN_CURSO, flg_current_status = true };

        IEnumerable<VehiculoViaje> current_available_vehiculo_viaje = await _databaseContext.Connection.QueryAsync<VehiculoViaje>(query, params_query);

        try
        {
            return current_available_vehiculo_viaje.SingleOrDefault();
        }
        catch
        {
            return null;
        }        
    }
}
