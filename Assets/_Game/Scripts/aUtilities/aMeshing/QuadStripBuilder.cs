using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using Orazum.Collections;

namespace Orazum.Meshing
{
    public struct QuadStripBuilder
    {
        private NativeArray<VertexData> _vertices;
        private NativeArray<short> _indices;
        private float3x2 _normalAndUV;

        private int2 _prevIndices;
        public QuadStripBuilder(NativeArray<VertexData> vertices, NativeArray<short> indices, float3x2 normalAndUV)
        {
            _vertices = vertices;
            _indices = indices;
            _normalAndUV = normalAndUV;
            _prevIndices = int2.zero;
        }

        public void ReorientVertices(int bufferLength)
        {
            for (int i = 0; i < bufferLength; i += 2)
            {
                CollectionUtilities.Swap(ref _vertices, i, i + 1);
            }   
        }

        public void Build(QuadStrip quadStrip, ref MeshBuffersIndexers buffersData)
        {
            Start(quadStrip[0], ref buffersData);
            for (int i = 0; i < quadStrip.QuadsCount; i++)
            {
                Continue(quadStrip[1], ref buffersData);
            }
        }

        public void Start(float3x2 lineSegment, ref MeshBuffersIndexers buffersData)
        {
            _prevIndices.x = AddVertex(lineSegment[0], ref buffersData);
            _prevIndices.y = AddVertex(lineSegment[1], ref buffersData);

        }

        private void DrawSegment(float3x2 p)
        {
            Debug.DrawRay(p[0], Vector3.up, Color.red, 5);
            Debug.DrawRay(p[1], Vector3.up, Color.red, 5);
        }

        public void Continue(float3x2 lineSegment, ref MeshBuffersIndexers buffersData)
        {
            int2 newIndices = int2.zero;
            newIndices.x = AddVertex(lineSegment[0], ref buffersData);
            newIndices.y = AddVertex(lineSegment[1], ref buffersData);

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

        private short AddVertex(float3 pos, ref MeshBuffersIndexers buffersData)
        { 
            VertexData vertex = new VertexData();
            vertex.position = pos;
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