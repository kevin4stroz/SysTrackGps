using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkerConsumer.Infraestructure.Entities;

[Table("vehiculo_vehiculo_status", Schema = "public")]
public class VehiculoVehiculoStatus
{
    [Key]
    public required Guid id_vehiculo_vehiculo_status { get; set; }
    public required Guid id_vehiculo { get; set; }
    public required Guid id_vehiculo_status { get; set; }
    public required bool flg_current_status { get; set; }
    public required DateTime created_date { get; set; }
}
