using Unity.Mathematics;

public static class WheelLookUpTable
{
    private static readonly int[] Data = new int[]
    {
        3, 11, 16, // BBL
        4, 10, 17, // BTL
        2, 8 , 23, // FBL
        5, 9 , 22, // FTL
        0, 12, 19, // BBR
        7, 13, 18, // BTR
        1, 15, 20, // FBR
        6, 14, 21  // FTR
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