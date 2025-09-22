using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkerConsumer.Infraestructure.Entities;

[Table("vehiculo", Schema = "public")]
public class Vehiculo
{
    [Key]
    public required Guid id_vehiculo { get; set; }
    public required string placa { get; set; }
    public required string color { get; set; }
    public required int modelo { get; set; }
    public required decimal cilindraje { get; set; }
    public required int capacidad_pasajeros { get; set; }
    public required int capacidad_carga { get; set; }
    public required DateTime created_date { get; set; }

}
