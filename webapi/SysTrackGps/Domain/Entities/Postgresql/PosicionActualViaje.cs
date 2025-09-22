using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysTrackGps.Domain.Entities.Postgresql;

[Table("posicion_actual_viaje", Schema = "public")]
public class PosicionActualViaje
{
    [Key]
    public Guid id_posicion_actual_viaje { get; set; }
    public double latitud { get; set; }
    public double longitud { get; set; }
    public Guid id_vehiculo_viaje { get; set; }
    public DateTime created_date { get; set; }
}

