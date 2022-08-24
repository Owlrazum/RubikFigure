using Unity.Mathematics;
using Unity.Collections;

using static Orazum.Math.MathUtilities;

namespace Orazum.Meshing
{
    public struct QuadStripBuilderVertexData
    {
        private NativeArray<VertexData> _vertices;
        private NativeArray<short> _indices;
        private float3x2 _normalAndUV;

        private int2 _prevIndices;
        public QuadStripBuilderVertexData(NativeArray<VertexData> vertices, NativeArray<short> indices)
        {
            _vertices = vertices;
            _indices = indices;
            _normalAndUV = float3x2.zero;
            _prevIndices = int2.zero;
        }

        public void SetNormalsAndUV(in float3x2 normalAndUV)
        {
            _normalAndUV = normalAndUV;
        }

        public void Start(float2x2 p, ref MeshBuffersData buffersData)
        {
            _prevIndices.x = AddVertex(p[0], ref buffersData);
            _prevIndices.y = AddVertex(p[1], ref buffersData);
        }

        public void Continue(float2x2 p, ref MeshBuffersData buffersData)
        {
            int2 newIndices = int2.zero;
            newIndices.x = AddVertex(p[0], ref buffersData);
            newIndices.y = AddVertex(p[1], ref buffersData);

            int4 quadIndices = new int4(_prevIndices, newIndices.yx);
            AddQuadIndices(quadIndices, ref buffersData);

            _prevIndices = newIndices;
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

        private short AddVertex(float2 pos, ref MeshBuffersData buffersData)
        { 
            VertexData vertex = new VertexData();
            vertex.position = x0z(pos);
            vertex.normal = _normalAndUV[0];
            vertex.uv = _normalAndUV[1].xy;
            
            _vertices[buffersData.Count.x++] = vertex;
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