using Unity.Mathematics;

public static class WheelLookUpTable
{
    private static readonly int[] Data = new int[]
    {
        3, 11, 21, // BBL
        5, 10, 20, // BTL
        2, 9 , 15, // FBL
        4, 8 , 14, // FTL
        1, 17, 23, // BBR
        7, 16, 22, // BTR
        0, 13, 19, // FBR
        6, 12, 18  // FTR
    };

    public static int3 GetCornerIndices(int cornerIndex)
    { 
        int3 cornerVertices = new int3(
            Data[cornerIndex * 3 + 0],
            Data[cornerIndex * 3 + 1],
            Data[cornerIndex * 3 + 2]
        );

        return cornerVertices;
    }
}