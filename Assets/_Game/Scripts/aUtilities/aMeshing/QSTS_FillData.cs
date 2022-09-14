using Unity.Mathematics;
using UnityEngine.Assertions;

public struct QSTS_FillData
{
    public enum ConstructType
    { 
        New, // QuadStripStart
        Continue // QuadStripContinue
    }
    public ConstructType Construct { get; private set; }

    public enum FillType
    {
        StartToEnd,
        FromStart,
        FromEnd,
        ToStart,
        ToEnd
    }
    public FillType Fill { get; private set; }

    public float2 LerpRange { get; set; }

    public QSTSFD_Radial Radial { get; set; }

    public QSTS_FillData(ConstructType construct, FillType fill, float2 lerpRange)
    {
        Construct = construct;
        Fill = fill;
        LerpRange = lerpRange;
        Radial = new QSTSFD_Radial(-1);
    }

    public QSTS_FillData(ConstructType construct, FillType fill, float2 lerpRange, in QSTSFD_Radial radial)
    {
        Construct = construct;
        Fill = fill;
        LerpRange = lerpRange;
        Radial = radial;
        Assert.IsTrue(radial.MaxLerpLength > 0);
    }

    public override string ToString()
    {
        return $"{Fill} {LerpRange.x:F2} {LerpRange.y:F2}" + (Radial.MaxLerpLength > 0 ? $"{Radial}" : "");
    }
}