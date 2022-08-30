using Unity.Mathematics;
using UnityEngine.Assertions;
using static QSTransSegment;

public struct QSTransSegFillData
{
    public float2 LerpRange { get; set; }
    public MeshConstructType ConstructType { get; set; }

    public QuadConstructType QuadType { get; set; }
    public RadialConstructType RadialType { get; set; }
    public QSTransSegFillRadialData RadialData { get; set; }

    public QSTransSegFillData(float2 lerpRange, MeshConstructType constructType)
    {
        LerpRange = lerpRange;
        ConstructType = constructType;
        QuadType = QuadConstructType.NewQuadStartToEnd;

        RadialType = RadialConstructType.Single;
        RadialData = new QSTransSegFillRadialData();
    }

    public override string ToString()
    {
        if (ConstructType == MeshConstructType.Quad)
        { 
            return $"Quad: {LerpRange.x:F2} {LerpRange.y:F2} {QuadType}";
        }
        else if (ConstructType == MeshConstructType.Radial)
        { 
            return $"Radial: {LerpRange.x:F2} {LerpRange.y:F2} {RadialType} {RadialData}";
        }
        else
        {
            return "Unknown Construct type";
        }
    }
}