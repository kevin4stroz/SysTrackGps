using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysTrackGps.Domain.Entities.Postgresql;

[Table("vehiculo_status", Schema = "public")]
public class VehiculoStatus
{
    [Key]
    public required Guid id_vehiculo_status { get; set; }
    public required string descripcion { get; set; }
    public required DateTime created_date { get; set; }
}