using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using Orazum.Collections;
using Orazum.Math;

namespace Orazum.Meshing
{
    public struct QuadStripBuilder
    {
        private NativeArray<VertexData> _vertices;
        private NativeArray<short> _indices;
        private float3x2 _normalAndUV;

        private int2 _prevIndices;
        private float3x4 _currentQuad;
        public QuadStripBuilder(in NativeArray<VertexData> vertices, in NativeArray<short> indices, in float3x2 normalAndUV)
        {
            _vertices = vertices;
            _indices = indices;
            _normalAndUV = normalAndUV;
            _prevIndices = int2.zero;
            
            _currentQuad = float3x4.zero;
        }

        public void ReorientVertices(int bufferLength)
        {
            for (int i = 0; i < bufferLength; i += 2)
            {
                _vertices.Swap(i, i + 1);
            }   
        }

        public void Build(QuadStrip quadStrip, ref MeshBuffersIndexers buffersIndexers)
        {
            Start(quadStrip[0], ref buffersIndexers);
            for (int i = 1; i < quadStrip.LineSegmentsCount; i++)
            {
                Continue(quadStrip[i], ref buffersIndexers);
            }
        }

        public void Build(NativeArray<float3x2> lineSegments, ref MeshBuffersIndexers buffersIndexers)
        { 
            Start(lineSegments[0], ref buffersIndexers);
            for (int i = 1; i < lineSegments.Length; i++)
            {
                Continue(lineSegments[i], ref buffersIndexers);
            }
        }

        public void Start(in float3x2 lineSegment, ref MeshBuffersIndexers buffersIndexers)
        {
            _prevIndices.x = AddVertex(lineSegment[0], ref buffersIndexers);//, new float2(0, 0.44f)
            _prevIndices.y = AddVertex(lineSegment[1], ref buffersIndexers);

            _currentQuad[2] = lineSegment[0];
            _currentQuad[3] = lineSegment[1];
        }

        public void Continue(in float3x2 lineSegment, ref MeshBuffersIndexers buffersIndexers)
        {
            int2 newIndices = int2.zero;
            newIndices.x = AddVertex(lineSegment[0], ref buffersIndexers);//, new float2(0, 0.44f)
            newIndices.y = AddVertex(lineSegment[1], ref buffersIndexers);

            _currentQuad[0] = _currentQuad[2];
            _currentQuad[1] = _currentQuad[3];
            _currentQuad[2] = lineSegment[0];
            _currentQuad[3] = lineSegment[1];

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

        private void AddQuadIndices(int4 quadIndicesToCheck, ref MeshBuffersIndexers buffersIndexers)
        {
            int4 quadIndices = quadIndicesToCheck;
            // CheckClockOrder(quadIndicesToCheck, out quadIndices);
            AddIndex(quadIndices.x, ref buffersIndexers);
            AddIndex(quadIndices.y, ref buffersIndexers);
            AddIndex(quadIndices.z, ref buffersIndexers);
            AddIndex(quadIndices.x, ref buffersIndexers);
            AddIndex(quadIndices.z, ref buffersIndexers);
            AddIndex(quadIndices.w, ref buffersIndexers);
        }

        private void CheckClockOrder(int4 quadIndices, out int4 correctedQuadIndices)
        {
            float3x3 t = new float3x3(
                 _currentQuad[0],
                 _currentQuad[1],
                 _currentQuad[2]
            );
            ClockOrderType c1 = MathUtils.GetTriangleClockOrder(t);
            
            t = new float3x3(
                 _currentQuad[0],
                 _currentQuad[2],
                 _currentQuad[3]
            );
            ClockOrderType c2 = MathUtils.GetTriangleClockOrder(t);
            if (c1 != c2)
            {
                Debug.LogWarning("triangles are in different orders");
            }
            if (c1 == ClockOrderType.AntiCW)
            {
                correctedQuadIndices = quadIndices.wzyx;
                Debug.Log($"corrected {quadIndices} to {correctedQuadIndices}");
            }
            else
            {
                correctedQuadIndices = quadIndices;
                Debug.Log($"no correction needed");
            }
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

        private short AddVertex(float3 pos, ref MeshBuffersIndexers buffersIndexers, float2 uv)
        { 
            VertexData vertex = new VertexData();
            vertex.position = pos;
            vertex.normal = _normalAndUV[0];
            vertex.uv = uv;
            
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