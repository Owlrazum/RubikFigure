using Unity.Mathematics;
using Orazum.Math;

public struct QSTSFD_Radial
{
    // RL: rotationLerp 
    // NQ: new quad
    // CQ: continue quad
    public enum RadialType
    {
        SingleRotation,
        DoubleRotation,
        SingleMove, // single means only one quadstrip, therefore use only start seg
        DoubleMove, // double means two quadstrips are involved, therefore use start and end segs
    }
    public RadialType Type { get; private set; }

    // negative lerpLength signifies invalid state
    public QSTSFD_Radial(float invalidUselessParameter)
    {
        MaxLerpLength = -1;

        Type = RadialType.SingleRotation;
        Points = float3x2.zero;
        AxisAngles = float4x2.zero;
        Resolution = -1;
    }

    public QSTSFD_Radial(
        RadialType radial,
        in float4x2 axisAngles,
        in float3x2 points,
        float lerpLength,
        int resolution)
    {
        Type = radial;
        AxisAngles = axisAngles;
        Points = points;

        MaxLerpLength = lerpLength;
        Resolution = resolution;
    }

    public float4x2 AxisAngles { get; private set; }
    public float3x2 Points { get; private set; }

    public int Resolution { get; set; }
    public float MaxLerpLength { get; set; }

    public bool IsRotationLerp
    {
        get
        {
            return Type == RadialType.SingleRotation ||
                   Type == RadialType.DoubleRotation;
        }
    }

    public bool IsMoveLerp
    {
        get
        {
            return Type == RadialType.SingleMove || Type == RadialType.SingleMove ||
                   Type == RadialType.DoubleMove || Type == RadialType.DoubleMove;
        }
    }

    public override string ToString()
    {
        return $"{MaxLerpLength:F2} {Points:F2} {Resolution} {AxisAngles:F2}";
    }
}