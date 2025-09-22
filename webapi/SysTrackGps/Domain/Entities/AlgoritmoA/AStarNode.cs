using System;

namespace SysTrackGps.Domain.Entities.AlgoritmoA;

public class AStarNode : IComparable<AStarNode>
{
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }
    public double GCost { get; set; }
    public double HCost { get; set; }
    public double FCost => GCost + HCost;
    public AStarNode? Parent { get; set; }

    public int CompareTo(AStarNode? other)
    {
        if (other == null) return 1;
        return FCost.CompareTo(other.FCost);
    }
}
