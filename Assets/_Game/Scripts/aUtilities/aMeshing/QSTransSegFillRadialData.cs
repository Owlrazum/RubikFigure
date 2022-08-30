using Unity.Mathematics;
public struct QSTransSegFillRadialData
{
    public int Resolution { get; set; }
    public float LerpLength { get; set; }
    public float StartLerpOffset { get; set; }

    public float3x2 Centers { get; set; }
    public float4x2 AxisAngles { get; set; }

    public override string ToString()
    {
        return $"{Resolution} {LerpLength:F2} {StartLerpOffset:F2} {Centers:F2} {AxisAngles:F2}";
    }
}