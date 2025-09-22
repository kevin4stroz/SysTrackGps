using System;

namespace SysTrackGps.Application.Dtos;

public class ResponseCreateVehiculo
{
    public Guid id_vehiculo { get; set; }
    public DateTime created_date { get; set; }
}
