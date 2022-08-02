using UnityEngine;

public static class WheelUtilities
{
    private static int[,] Data = new int[,]
    {
        {3, 11, 21}, // BBL
        {1, 17, 23}, // BBR
        {5, 10, 20}, // BTL
        {7, 16, 22}, // BTR
        {2, 9 , 15}, // FBL
        {0, 13, 19}, // FBR
        {4, 8 , 14}, // FTL
        {6, 12, 18}  // FTR
    };

    private enum VertexCorner
    { 
        BBL = 0,
        BBR = 1,
        BTL = 2,
        BTR = 3,
        FBL = 4,
        FBR = 5,
        FTL = 6,
        FTR = 7
    }

    public static CircleRays CurrentRays;

    public static void TestSegment(Vector3[] vertices, float speed, Vector2Int rayIndices)
    {
        for (int i = 0; i < 8; i++)
        {
            (int v1, int v2, int v3) corner = GetCornerIndices(i);

            Vector3 delta = Vector3.zero;
            if (i % 2 == 0) // left ray
            {
                delta = CurrentRays.GetRay(rayIndices.x) * speed * Time.deltaTime;;
            }
            else  // right ray
            { 
                delta = CurrentRays.GetRay(rayIndices.y) * speed * Time.deltaTime; 
            }

            Debug.Log(delta);

            vertices[corner.v1] += delta;
            vertices[corner.v2] += delta;
            vertices[corner.v3] += delta;
        }
    }

    private static (int v1, int v2, int v3) GetCornerIndices(int cornerIndex)
    {
        (int v1, int v2, int v3) cornerVertices;
        cornerVertices.v1 = Data[cornerIndex, 0];
        cornerVertices.v2 = Data[cornerIndex, 1];
        cornerVertices.v3 = Data[cornerIndex, 2];

        return cornerVertices;
    }
}