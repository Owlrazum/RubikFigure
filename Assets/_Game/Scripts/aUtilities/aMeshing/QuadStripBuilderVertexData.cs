using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using static Orazum.Math.MathUtilities;

namespace Orazum.Meshing
{
    public struct QuadStripBuilderVertexData
    {
        private NativeArray<VertexData> _vertices;
        private NativeArray<short> _indices;
        private float3x2 _normalAndUV;

        private int2 _prevIndices;
        public QuadStripBuilderVertexData(NativeArray<VertexData> vertices, NativeArray<short> indices, float3x2 normalAndUV)
        {
            _vertices = vertices;
            _indices = indices;
            _normalAndUV = normalAndUV;
            _prevIndices = int2.zero;
        }

        public void SetNormalAndUV(float3x2 normalAndUV)
        {
            _normalAndUV = normalAndUV;
        }

        public void Build(QuadStrip quadStrip, ref MeshBuffersIndexers buffersData)
        {
            Start(quadStrip[0], ref buffersData);
            for (int i = 0; i < quadStrip.QuadsCount; i++)
            {
                Continue(quadStrip[1], ref buffersData);
            }
        }

        public void Start(float2x2 p, ref MeshBuffersIndexers buffersData)
        {
            _prevIndices.x = AddVertex(p[0], ref buffersData);
            _prevIndices.y = AddVertex(p[1], ref buffersData);

        }

        private void DrawSegment(float2x2 p)
        {
            Debug.DrawRay(x0z(p[0]), Vector3.up, Color.red, 5);
            Debug.DrawRay(x0z(p[1]), Vector3.up, Color.red, 5);
        }

        public void Continue(float2x2 p, ref MeshBuffersIndexers buffersData)
        {
            int2 newIndices = int2.zero;
            newIndices.x = AddVertex(p[0], ref buffersData);
            newIndices.y = AddVertex(p[1], ref buffersData);

            int4 quadIndices = new int4(_prevIndices, newIndices.yx);
            AddQuadIndices(quadIndices, ref buffersData);

            _prevIndices = newIndices;
        }

        private void AddQuadIndices(int4 quadIndices, ref MeshBuffersIndexers buffersData)
        {
            AddIndex(quadIndices.x, ref buffersData);
            AddIndex(quadIndices.y, ref buffersData);
            AddIndex(quadIndices.z, ref buffersData);
            AddIndex(quadIndices.x, ref buffersData);
            AddIndex(quadIndices.z, ref buffersData);
            AddIndex(quadIndices.w, ref buffersData);
        }

        private short AddVertex(float2 pos, ref MeshBuffersIndexers buffersData)
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

        private void AddIndex(int vertexIndex, ref MeshBuffersIndexers buffersData)
        {
            _indices[buffersData.Count.y++] = (short)vertexIndex;
        }
    }
}