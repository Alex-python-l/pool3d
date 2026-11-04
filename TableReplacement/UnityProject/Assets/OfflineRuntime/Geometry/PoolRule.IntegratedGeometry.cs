using Pool3D.Offline.Geometry;

public partial class PoolRule
{
    private TableGeometryConfig integratedGeometry;
    internal static TableGeometryConfig LegacyGeometrySnapshot {get{return TableGeometryConfig.Legacy(blockersPosition,holePosition);}}
    public TableGeometryConfig Geometry { get { return integratedGeometry ?? (integratedGeometry=TableGeometryConfig.Legacy(blockersPosition,holePosition)); } }
    public void ConfigureGeometry(TableGeometryConfig geometry) { geometry.Validate(); integratedGeometry=geometry.Copy(); }
    protected float[] GInside {get{return Geometry.IsLegacy?insideTableCorners:Geometry.Inside;}}
    protected float[] GOutside {get{return Geometry.IsLegacy?outsideTableCorners:Geometry.Outside;}}
    protected float[] GFree {get{return Geometry.IsLegacy?freeAreaCorners:Geometry.Free;}}
    protected float[] GBorder {get{return Geometry.IsLegacy?insideBorder:Geometry.Border;}}
    protected float[] GBlockers {get{return Geometry.IsLegacy?blockersPosition:Geometry.Blockers;}}
    protected float[] GHoles {get{return Geometry.IsLegacy?holePosition:Geometry.Holes;}}
}

public partial class PoolTable
{
    public TableGeometryConfig Geometry {get{return rule.Geometry;}}
    private void EnsureCollisionCapacity()
    {
        int capacity=System.Math.Max(30,System.Math.Max(balls.Length,System.Math.Max(blockers.Length,holes.Length)));
        if(nextHitTime==null||nextHitTime.GetLength(1)<capacity||nextHitTime.GetLength(2)<capacity) nextHitTime=new float[6,capacity,capacity];
    }
}
