using Unity.Mathematics;
using Unity.Collections;

using static Orazum.Math.MathUtils;

namespace Orazum.Meshing
{
    public struct QuadStripBuilder
    {
        private NativeArray<float3> _vertices;
        private NativeArray<short> _indices;

        private int2 _prevIndices;
        public QuadStripBuilder(NativeArray<float3> vertices, NativeArray<short> indices)
        {
            _vertices = vertices;
            _indices = indices;
            _prevIndices = int2.zero;
        }

        public void Build(QuadStrip quadStrip, ref MeshBuffersIndexers indexers)
        {
            Start(quadStrip[0], ref indexers);
            for (int i = 0; i < quadStrip.QuadsCount; i++)
            {
                Continue(quadStrip[1], ref indexers);
            }
        }

        public void Start(float2x2 p, ref MeshBuffersIndexers indexers)
        {
            _prevIndices.x = AddVertex(p[0], ref indexers);
            _prevIndices.y = AddVertex(p[1], ref indexers);
        }

        public void Start(float3x2 p, ref MeshBuffersIndexers indexers)
        { 
            _prevIndices.x = AddVertex(p[0], ref indexers);
            _prevIndices.y = AddVertex(p[1], ref indexers);
        }

        public void Continue(float2x2 p, ref MeshBuffersIndexers indexers)
        {
            int2 newIndices = int2.zero;
            newIndices.x = AddVertex(p[0], ref indexers);
            newIndices.y = AddVertex(p[1], ref indexers);

            int4 quadIndices = new int4(_prevIndices, newIndices.yx);
            AddQuadIndices(quadIndices, ref indexers);

            _prevIndices = newIndices;
        }

        public void Continue(float3x2 p, ref MeshBuffersIndexers indexers)
        { 
            int2 newIndices = int2.zero;
            newIndices.x = AddVertex(p[0], ref indexers);
            newIndices.y = AddVertex(p[1], ref indexers);

            int4 quadIndices = new int4(_prevIndices, newIndices.yx);
            AddQuadIndices(quadIndices, ref indexers);

            _prevIndices = newIndices;
        }

        private void AddQuadIndices(int4 quadIndices, ref MeshBuffersIndexers indexers)
        {
            AddIndex(quadIndices.x, ref indexers);
            AddIndex(quadIndices.y, ref indexers);
            AddIndex(quadIndices.z, ref indexers);
            AddIndex(quadIndices.x, ref indexers);
            AddIndex(quadIndices.z, ref indexers);
            AddIndex(quadIndices.w, ref indexers);
        }

        private short AddVertex(float2 pos, ref MeshBuffersIndexers indexers)
        { 
            _vertices[indexers.Count.x++] = x0z(pos);
            short addedVertexIndex = (short)indexers.LocalCount.x;
            indexers.LocalCount.x++;

            return addedVertexIndex;
        }

        private short AddVertex(float3 pos, ref MeshBuffersIndexers indexers)
        { 
            _vertices[indexers.Count.x++] = pos;
            short addedVertexIndex = (short)indexers.LocalCount.x;
            indexers.LocalCount.x++;

            return addedVertexIndex;
        }

        private void AddIndex(int vertexIndex, ref MeshBuffersIndexers buffersData)
        {
            _indices[buffersData.Count.y++] = (short)vertexIndex;
        }
    }
}