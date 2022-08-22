using Unity.Collections;
using Unity.Mathematics;

namespace Orazum.Meshing
{
    public struct CubeStripSegmented
    {
        private NativeArray<float3> _vertices;
        private NativeArray<short> _indices;

        private int4 _prevCubeStripIndices;
        public CubeStripSegmented(NativeArray<float3> vertices, NativeArray<short> indices)
        {
            _vertices = vertices;
            _indices = indices;

            _prevCubeStripIndices = int4.zero;
        }

        public void Start(float3x4 p, ref MeshBuffersData buffersData)
        {
            buffersData.LocalCount = int2.zero;
            _prevCubeStripIndices.x = AddVertex(p[0], ref buffersData);
            _prevCubeStripIndices.y = AddVertex(p[1], ref buffersData);
            _prevCubeStripIndices.z = AddVertex(p[2], ref buffersData);
            _prevCubeStripIndices.w = AddVertex(p[3], ref buffersData);
            AddQuadIndices(_prevCubeStripIndices, ref buffersData);
        }

        public void Continue(float3x4 p, ref MeshBuffersData buffersData)
        {
            int4 newCubeStripIndices = int4.zero;
            newCubeStripIndices.x = AddVertex(p[0], ref buffersData);
            newCubeStripIndices.y = AddVertex(p[1], ref buffersData);
            newCubeStripIndices.z = AddVertex(p[2], ref buffersData);
            newCubeStripIndices.w = AddVertex(p[3], ref buffersData);

            int4 quadIndices;
            quadIndices = new int4(newCubeStripIndices.xy, _prevCubeStripIndices.yx);
            AddQuadIndices(quadIndices, ref buffersData);
            quadIndices = new int4(newCubeStripIndices.yz, _prevCubeStripIndices.zy);
            AddQuadIndices(quadIndices, ref buffersData);
            quadIndices = new int4(newCubeStripIndices.zw, _prevCubeStripIndices.wz);
            AddQuadIndices(quadIndices, ref buffersData);
            quadIndices = new int4(_prevCubeStripIndices.xw, newCubeStripIndices.wx);
            AddQuadIndices(quadIndices, ref buffersData);

            buffersData.LocalCount = int2.zero;
            _prevCubeStripIndices.x = AddVertex(p[0], ref buffersData);
            _prevCubeStripIndices.y = AddVertex(p[1], ref buffersData);
            _prevCubeStripIndices.z = AddVertex(p[2], ref buffersData);
            _prevCubeStripIndices.w = AddVertex(p[3], ref buffersData);
        }

        public void Finish(float3x4 p, ref MeshBuffersData buffersData)
        {
            int4 newCubeStripIndices = int4.zero;
            newCubeStripIndices.x = AddVertex(p[0], ref buffersData);
            newCubeStripIndices.y = AddVertex(p[1], ref buffersData);
            newCubeStripIndices.z = AddVertex(p[2], ref buffersData);
            newCubeStripIndices.w = AddVertex(p[3], ref buffersData);

            int4 quadIndices;
            quadIndices = new int4(newCubeStripIndices.xy, _prevCubeStripIndices.yx);
            AddQuadIndices(quadIndices, ref buffersData);
            quadIndices = new int4(newCubeStripIndices.yz, _prevCubeStripIndices.zy);
            AddQuadIndices(quadIndices, ref buffersData);
            quadIndices = new int4(newCubeStripIndices.zw, _prevCubeStripIndices.wz);
            AddQuadIndices(quadIndices, ref buffersData);
            quadIndices = new int4(_prevCubeStripIndices.xw, newCubeStripIndices.wx);
            AddQuadIndices(quadIndices, ref buffersData);
            quadIndices = new int4(newCubeStripIndices.wzyx);
            AddQuadIndices(quadIndices, ref buffersData);

            _prevCubeStripIndices = newCubeStripIndices;
        }

        private void AddQuadIndices(int4 quadIndices, ref MeshBuffersData buffersData)
        {
            AddIndex(quadIndices.x, ref buffersData);
            AddIndex(quadIndices.y, ref buffersData);
            AddIndex(quadIndices.z, ref buffersData);
            AddIndex(quadIndices.x, ref buffersData);
            AddIndex(quadIndices.z, ref buffersData);
            AddIndex(quadIndices.w, ref buffersData);
        }

        private short AddVertex(float3 pos, ref MeshBuffersData buffersData)
        {
            _vertices[buffersData.Count.x++] = pos;
            short addedVertexIndex = (short)buffersData.LocalCount.x;
            buffersData.LocalCount.x++;

            return addedVertexIndex;
        }

        private void AddIndex(int vertexIndex, ref MeshBuffersData buffersData)
        {
            _indices[buffersData.Count.y++] = (short)vertexIndex;
        }
    }
}