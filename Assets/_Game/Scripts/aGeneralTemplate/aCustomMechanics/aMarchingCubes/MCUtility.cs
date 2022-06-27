using Unity.Mathematics;
using Unity.Collections;

namespace MarchingCubes
{ 
    public static class MCUtility
    {
        /// <summary>
        /// Interpolates the vertex's position.
        /// p - corner.
        /// v - density.
        /// isolevel - The density level where a surface will be created.
        /// Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        /// <returns>The interpolated vertex's position</returns>
        public static float3 VertexInterpolate(float3 p1, float3 p2, float v1, float v2, float isolevel)
        {
            return p1 + (isolevel - v1) * (p2 - p1) / (v2 - v1);
        }

        public static byte CalculateCubeIndex(VoxelCorners<(float3 pos, byte value)> voxelCorners, byte isoLevel)
        { 
            byte cubeIndex = (byte)math.select(0, 1,   voxelCorners.Corner1.value < isoLevel);
            cubeIndex     |= (byte)math.select(0, 2,   voxelCorners.Corner2.value < isoLevel);
            cubeIndex     |= (byte)math.select(0, 4,   voxelCorners.Corner3.value < isoLevel);
            cubeIndex     |= (byte)math.select(0, 8,   voxelCorners.Corner4.value < isoLevel);
            cubeIndex     |= (byte)math.select(0, 16,  voxelCorners.Corner5.value < isoLevel);
            cubeIndex     |= (byte)math.select(0, 32,  voxelCorners.Corner6.value < isoLevel);
            cubeIndex     |= (byte)math.select(0, 64,  voxelCorners.Corner7.value < isoLevel);
            cubeIndex     |= (byte)math.select(0, 128, voxelCorners.Corner8.value < isoLevel);

            return cubeIndex;
        }

        public static VoxelCorners<(float3, byte)> GetVoxelCorners(
            ScalarField<byte> scalarField, 
            NativeArray<GridCell> distanceField, 
            int3 localPosition,
            float cellSize
        )
        { 
            VoxelCorners<(float3, byte)> voxelCorners = new VoxelCorners<(float3, byte)>();
            for (int i = 0; i < 8; i++)
            {
                int3 voxelCorner = localPosition + LookupTables.CubeCorners[i];
                if (scalarField.TryGetData(voxelCorner, out byte data))
                {
                    int distanceFieldIndex = 
                        scalarField.GetDistanceIndex(new int2(voxelCorner.x, voxelCorner.z));

                    float3 pos = distanceField[distanceFieldIndex].LocalPos;
                    pos.y = cellSize * voxelCorner.y;
                    voxelCorners[i] = (pos, data);
                }
            }
            return voxelCorners;
        }

        public static VertexList GenerateVertexList(
            VoxelCorners<(float3 pos, byte value)> voxelCorners, 
            int edgeIndex, 
            byte isoLevel
        )
        { 
            VertexList vertexList = new VertexList();

            for (int i = 0; i < 12; i++)
            {
                if ((edgeIndex & (1 << i)) == 0) { continue; }

                int edgeStartIndex = LookupTables.EdgeIndexTable[2 * i + 0];
                int edgeEndIndex = LookupTables.EdgeIndexTable[2 * i + 1];

                float3 corner1 = voxelCorners[edgeStartIndex].pos;
                float3 corner2 = voxelCorners[edgeEndIndex].pos;

                float density1 = voxelCorners[edgeStartIndex].value / 255f;
                float density2 = voxelCorners[edgeEndIndex].value / 255f;

                vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isoLevel / 255f);
            }

            return vertexList;
        }
    }
}