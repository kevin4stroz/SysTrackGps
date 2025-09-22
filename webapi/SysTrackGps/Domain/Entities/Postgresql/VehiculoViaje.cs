using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysTrackGps.Domain.Entities.Postgresql;

[Table("vehiculo_viaje", Schema = "public")]
public class VehiculoViaje
{
    [Key]
    public required Guid id_vehiculo_viaje { get; set; }
    public required Guid id_vehiculo_vehiculo_status { get; set; }
    public required string origen { get; set; }
    public required string destino { get; set; }
    public required DateTime created_date { get; set; }
    public required string key_redis_ruta { get; set; }
}
