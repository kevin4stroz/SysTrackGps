using System;

namespace SysTrackGps.Application.Dtos;

public class RecvCoordsDto
{
    public required Guid id_vehiculo { get; set; }
    public required double latitud { get; set; }
    public required double longitud { get; set; }
}
