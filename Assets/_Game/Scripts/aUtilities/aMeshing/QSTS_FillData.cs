using Unity.Mathematics;
using UnityEngine.Assertions;

public struct QSTS_FillData
{
    public enum FillType
    {
        NewStartToEnd,
        NewFromStart,
        NewFromEnd,
        NewToStart,
        NewToEnd,
        ContinueStartToEnd,
        ContinueFromStart
    }
    public FillType Fill { get; private set; }
    public float2 LerpRange { get; set; }

    public QSTSFD_Radial Radial { get; set; }

    public QSTS_FillData(FillType fill, float2 lerpRange)
    {
        Fill = fill;
        LerpRange = lerpRange;
        Radial = new QSTSFD_Radial(-1);
    }

    public QSTS_FillData(FillType fill, float2 lerpRange, in QSTSFD_Radial radial)
    {
        Fill = fill;
        LerpRange = lerpRange;
        Radial = radial;
        Assert.IsTrue(radial.LerpLength > 0);
    }

    public override string ToString()
    {
        return $"{Fill} {LerpRange.x:F2} {LerpRange.y:F2}" + (Radial.LerpLength > 0 ? $"{Radial}" : "");
    }
}