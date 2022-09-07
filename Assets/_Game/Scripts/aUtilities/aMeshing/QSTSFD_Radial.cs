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
        DoubleRotationLerp,
        MoveLerpWithMiddle,
        MoveLerp
    }
    public RadialType Type { get; private set; }
    public VertOrderType VertOrder { get; set; }
    public ClockOrderType ClockOrder { get; set; }

    // negative lerpLength signifies invalid state
    public QSTSFD_Radial(float lerpLength)
    {
        LerpLength = lerpLength;

        Type = RadialType.SingleRotationLerp;
        Points = float3x2.zero;
        AxisAngles = float4x2.zero;
        Resolution = -1;

        VertOrder = VertOrderType.Up;
        ClockOrder = ClockOrderType.CW;
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

        VertOrder = VertOrderType.Up;
        ClockOrder = ClockOrderType.CW;
    }

    public float4x2 AxisAngles { get; private set; }
    public float3x2 Points { get; private set; }

    public int Resolution { get; set; }
    public float LerpLength { get; set; }

    public bool IsRotationLerp
    {
        get
        {
            return Type == RadialType.SingleRotationLerp || Type == RadialType.DoubleRotationLerp;
        }
    }
    public bool IsMoveLerp
    {
        get
        {
            return Type == RadialType.MoveLerp || Type == RadialType.MoveLerpWithMiddle;
        }
    }

    public override string ToString()
    {
        return $"{LerpLength:F2} {Points:F2} {Resolution} {AxisAngles:F2}";
    }
}