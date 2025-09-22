using System;
using SysTrackGps.Domain.Entities.Redis;

namespace SysTrackGps.Domain.Entities.AlgoritmoA;

public class AStarResult
{
    public List<LocalidadRedis> Path { get; set; } = new List<LocalidadRedis>();
    public double TotalDistance { get; set; }
    public int NodesVisited { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}