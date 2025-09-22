using System;

namespace SysTrackGps.Domain.Entities.RabbitMq;

public class MessageToProcess
{
    public Guid id_vehiculo_viaje { get; set; }
    public Guid id_vehiculo { get; set; }
    public double latitud { get; set; }
    public double longitud { get; set; }
}
