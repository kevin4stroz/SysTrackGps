using System;

namespace SysTrackGps.Application.Dtos;

public class VehiculoCreateDto
{
    public required string placa { get; set; }
    public required string color { get; set; }
    public required int modelo { get; set; }
    public required decimal cilindraje { get; set; }
    public required int capacidad_pasajeros { get; set; }
    public required int capacidad_carga { get; set; }
}
