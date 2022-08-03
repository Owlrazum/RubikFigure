using System;
using Unity.Mathematics;

public struct SegmentPoint
{
    public float3 BBL;
    public float3 BTL;
    public float3 FBL;
    public float3 FTL;

    public float3 BBR;
    public float3 BTR;
    public float3 FBR;
    public float3 FTR;

    public float3 GetCornerPosition(int cornerIndex)
    {
        switch (cornerIndex)
        { 
            case 0:
                return BBL;
            case 1:
                return BTL;
            case 2:
                return FBL;
            case 3:
                return FTL;
            case 4:
                return BBR;
            case 5:
                return BTR;
            case 6:
                return FBR;
            case 7:
                return FTR;
            default:
                throw new ArgumentOutOfRangeException("cornerIndex should be in [0 .. 7]");
        }
    }
}