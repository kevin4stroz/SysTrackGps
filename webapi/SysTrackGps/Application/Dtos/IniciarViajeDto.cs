using System;

namespace SysTrackGps.Application.Dtos;

public class IniciarViajeDto
{
    public required Guid id_vehiculo { get; set; }
    public required string localidad_origen { get; set; }
    public required string localidad_destino { get; set; }
}
