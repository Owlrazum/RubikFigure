using Unity.Mathematics;
using static QSTransSegment;

public struct QSTransSegFillData
{
    public float2 LerpRange { get; private set; }
    public QuadConstructType ConstructType { get; private set; }

    public QSTransSegFillData(float2 lerpRange, QuadConstructType constructType)
    {
        LerpRange = lerpRange;
        ConstructType = constructType;
    }

    public void SetLerpRange(float2 lerpRange)
    {
        LerpRange = lerpRange;
    }

    public override string ToString()
    {
        return $"{LerpRange.x:F2} {LerpRange.y:F2} {ConstructType}";
    }
}