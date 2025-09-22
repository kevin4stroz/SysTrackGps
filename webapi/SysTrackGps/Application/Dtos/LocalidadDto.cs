using System;

namespace SysTrackGps.Application.Dtos;

public class LocalidadDto
{
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }
}
