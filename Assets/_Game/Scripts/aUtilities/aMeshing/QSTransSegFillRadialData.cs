using Unity.Mathematics;
public struct QSTransSegFillRadialData
{
    public float LerpLength { get; set; }
    public float StartLerpOffset { get; set; }

    public int Resolution { get; set; }
    public float3x2 Centers { get; set; }
    public float4x2 AxisAngles { get; set; }

    public override string ToString()
    {
        return $"{LerpLength:F2} {Centers:F2} {Resolution} {AxisAngles:F2}";
    }
}