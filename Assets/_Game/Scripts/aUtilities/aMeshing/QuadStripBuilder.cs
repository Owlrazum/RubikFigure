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

        public void Build(QuadStrip quadStrip, ref MeshBuffersIndexers buffersIndexers)
        {
            Start(quadStrip[0], ref buffersIndexers);
            for (int i = 0; i < quadStrip.QuadsCount; i++)
            {
                Continue(quadStrip[1], ref buffersIndexers);
            }
        }

        public void Start(in float3x2 lineSegment, ref MeshBuffersIndexers buffersIndexers)
        {
            _prevIndices.x = AddVertex(lineSegment[0], ref buffersIndexers);
            _prevIndices.y = AddVertex(lineSegment[1], ref buffersIndexers);

        }

        public void Continue(in float3x2 lineSegment, ref MeshBuffersIndexers buffersIndexers)
        {
            int2 newIndices = int2.zero;
            newIndices.x = AddVertex(lineSegment[0], ref buffersIndexers);
            newIndices.y = AddVertex(lineSegment[1], ref buffersIndexers);

            int4 quadIndices = new int4(_prevIndices, newIndices.yx);
            AddQuadIndices(quadIndices, ref buffersIndexers);

            _prevIndices = newIndices;
        }

        public void Add(in float3x2 lineSegment, ref MeshBuffersIndexers buffersIndexers, ref bool isStripStarted)
        { 
            if (!isStripStarted)
            {
                isStripStarted = true;
                Start(lineSegment, ref buffersIndexers);
            }
            else
            { 
                Continue(lineSegment, ref buffersIndexers);
            }
        }

        private void AddQuadIndices(int4 quadIndices, ref MeshBuffersIndexers buffersIndexers)
        {
            AddIndex(quadIndices.x, ref buffersIndexers);
            AddIndex(quadIndices.y, ref buffersIndexers);
            AddIndex(quadIndices.z, ref buffersIndexers);
            AddIndex(quadIndices.x, ref buffersIndexers);
            AddIndex(quadIndices.z, ref buffersIndexers);
            AddIndex(quadIndices.w, ref buffersIndexers);
        }

        private short AddVertex(float3 pos, ref MeshBuffersIndexers buffersIndexers)
        { 
            VertexData vertex = new VertexData();
            vertex.position = pos;
            vertex.normal = _normalAndUV[0];
            vertex.uv = _normalAndUV[1].xy;
            
            _vertices[buffersIndexers.Count.x++] = vertex;
            short addedVertexIndex = (short)buffersIndexers.LocalCount.x;
            buffersIndexers.LocalCount.x++;

            return addedVertexIndex;
        }

        private void AddIndex(int vertexIndex, ref MeshBuffersIndexers buffersIndexers)
        {
            _indices[buffersIndexers.Count.y++] = (short)vertexIndex;
        }

        private void DrawSegment(float3x2 p)
        {
            Debug.DrawRay(p[0], Vector3.up, Color.red, 5);
            Debug.DrawRay(p[1], Vector3.up, Color.red, 5);
        }
    }
}