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
        PrimaryAxisAngle = float4.zero;
        SecondaryAngle = 0;
        RotationCenter = float3.zero;
        Resolution = -1;
    }

    public QSTSFD_Radial(
        RadialType radial,
        in float4 primaryAxisAngle,
        in float secondaryAngle,
        in float3 rotationCenter,
        float lerpLength,
        int resolution)
    {
        Type = radial;
        PrimaryAxisAngle = primaryAxisAngle;
        SecondaryAngle = secondaryAngle;
        RotationCenter = rotationCenter;

        MaxLerpLength = lerpLength;
        Resolution = resolution;
    }

    public float4 PrimaryAxisAngle { get; private set; }
    public float SecondaryAngle { get; private set; }
    public float3 RotationCenter { get; private set; }

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
        return $"{MaxLerpLength:F2} {RotationCenter:F2} {Resolution} {PrimaryAxisAngle:F2}";
    }
}