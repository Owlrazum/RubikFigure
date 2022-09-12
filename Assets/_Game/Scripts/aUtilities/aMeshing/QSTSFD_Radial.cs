using Unity.Mathematics;
using Orazum.Math;

public struct QSTSFD_Radial
{
    // RL: rotationLerp 
    // NQ: new quad
    // CQ: continue quad
    public enum RadialType
    {
        SingleRotationLerp,
        DoubleRotationLerpDown,
        DoubleRotationLerpUp,
        SingleMoveLerpDown, // single means only one quadstrip, therefore use only start seg
        SingleMoveLerpUp,
        DoubleMoveLerpDown, // double means two quadstrips are involved, therefore use start and end segs
        DoubleMoveLerpUp
    }
    public RadialType Type { get; private set; }

    // negative lerpLength signifies invalid state
    public QSTSFD_Radial(float invalidUselessParameter)
    {
        LerpLength = -1;

        Type = RadialType.SingleRotationLerp;
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

        LerpLength = lerpLength;
        Resolution = resolution;
    }

    public float4x2 AxisAngles { get; private set; }
    public float3x2 Points { get; private set; }

    public int Resolution { get; set; }
    public float LerpLength { get; set; }

    public bool IsRotationLerp
    {
        get
        {
            return Type == RadialType.SingleRotationLerp || 
                   Type == RadialType.DoubleRotationLerpDown || 
                   Type == RadialType.DoubleRotationLerpUp;
        }
    }

    public bool IsMoveLerp
    {
        get
        {
            return Type == RadialType.SingleMoveLerpDown || Type == RadialType.SingleMoveLerpDown ||
                   Type == RadialType.DoubleMoveLerpDown || Type == RadialType.DoubleMoveLerpDown;
        }
    }

    public override string ToString()
    {
        return $"{LerpLength:F2} {Points:F2} {Resolution} {AxisAngles:F2}";
    }
}