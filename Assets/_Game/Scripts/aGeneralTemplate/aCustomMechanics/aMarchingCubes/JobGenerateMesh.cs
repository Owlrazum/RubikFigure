using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace MarchingCubes
{
    [BurstCompile]
    public struct JobGenerateMesh : IJob
    {
        [ReadOnly]
        public ScalarField<byte> InputScalarField;

        [ReadOnly]
        public NativeArray<GridCell> InputDistanceField;

        [WriteOnly]
        public NativeArray<VertexData> OutputVertices;
        [WriteOnly]
        public NativeArray<ushort> OutputTriangles;

        public NativeArray<int> VertexCount;

        public float CellSize;
        
        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        public byte IsoLevel;

        public void Execute()
        {
            for (int y = 0; y < InputScalarField.Height - 1; y++)
            {
                for (int z = 0; z < InputScalarField.Depth - 1; z++)
                {
                    for (int x = 0; x < InputScalarField.Width - 1; x++)
                    {
                        int3 scalarFieldLocalPos = new int3(x, y, z);

                        VoxelCorners<(float3, byte)> voxelCorners = MCUtility.GetVoxelCorners(
                            InputScalarField,
                            InputDistanceField,
                            scalarFieldLocalPos,
                            CellSize
                        );

                        byte cubeIndex = MCUtility.CalculateCubeIndex(voxelCorners, IsoLevel);
                        if (cubeIndex == 0 || cubeIndex == 255)
                        {
                            continue;
                        }

                        int edgeIndex = LookupTables.EdgeTable[cubeIndex];

                        VertexList vertexList = MCUtility.GenerateVertexList(
                            voxelCorners, 
                            edgeIndex, 
                            IsoLevel
                        );

                        int rowIndex = 15 * cubeIndex;

                        for (int i = 0; LookupTables.TriangleTable[rowIndex + i] != -1 && i < 15; i += 3)
                        {
                            float3 vertex1 = vertexList[LookupTables.TriangleTable[rowIndex + i + 0]];
                            float3 vertex2 = vertexList[LookupTables.TriangleTable[rowIndex + i + 1]];
                            float3 vertex3 = vertexList[LookupTables.TriangleTable[rowIndex + i + 2]];

                            if (!vertex1.Equals(vertex2) && !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3))
                            {
                                float3 normal = math.normalize(math.cross(vertex2 - vertex1, vertex3 - vertex1));

                                int triangleIndex = VertexCount[0]++ * 3;

                                OutputVertices[triangleIndex + 0] = new VertexData(vertex1, normal);
                                OutputTriangles[triangleIndex + 0] = (ushort)(triangleIndex + 0);

                                OutputVertices[triangleIndex + 1] = new VertexData(vertex2, normal);
                                OutputTriangles[triangleIndex + 1] = (ushort)(triangleIndex + 1);

                                OutputVertices[triangleIndex + 2] = new VertexData(vertex3, normal);
                                OutputTriangles[triangleIndex + 2] = (ushort)(triangleIndex + 2);
                            }
                        }
                    }
                }
            }
        }
    }
}
