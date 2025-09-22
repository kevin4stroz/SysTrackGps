using System;

namespace SysTrackGps.Domain.Entities.Redis;

public class LocalidadRedis
{
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }

    //public List<(LocalidadRedis Neighbor, double Distance)> Neighbors { get; set; } = new();
}
