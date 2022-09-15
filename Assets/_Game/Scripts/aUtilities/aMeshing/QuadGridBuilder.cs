using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using Orazum.Collections;

namespace Orazum.Meshing
{
    public struct QuadGridBuilder
    {
        private NativeArray<VertexData> _vertices;
        private NativeArray<short> _indices;
        private float2 _uv;

        private NativeArray<int> _prevIndices;
        public QuadGridBuilder(in NativeArray<VertexData> vertices, in NativeArray<short> indices, in float2 uv)
        {
            _vertices = vertices;
            _indices = indices;
            _uv = uv;
            _prevIndices = new NativeArray<int>();
        }

        public void Start(in NativeArray<float3> gridDim, ref MeshBuffersIndexers buffersIndexers)
        {
            _prevIndices = new NativeArray<int>(gridDim.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < gridDim.Length; i++)
            {
                _prevIndices[i] = AddVertex(gridDim[i], ref buffersIndexers);
            }
        }

        public void Continue(in NativeArray<float3> gridDim, ref MeshBuffersIndexers buffersIndexers)
        {
            Assert.IsTrue(_prevIndices.Length == gridDim.Length, "GridBuilder supports only gridDims of the same size");
            int2 newIndices = int2.zero;
            int2 indexer = new int2(0, 1);
            for (int i = 0; i < gridDim.Length - 1; i++)
            {
                int2 prevIndices = new int2(
                    _prevIndices[indexer.x],
                    _prevIndices[indexer.y]
                );

                newIndices = new int2(
                    AddVertex(gridDim[i], ref buffersIndexers),
                    AddVertex(gridDim[i + 1], ref buffersIndexers)
                );              

                int4 quadIndices = new int4(prevIndices, newIndices.yx);
                AddQuadIndices(quadIndices, ref buffersIndexers);

                _prevIndices[indexer.x] = newIndices.x;
                _prevIndices[indexer.y] = newIndices.y;
                indexer.x++;
                indexer.y++;
            }
        }

        public void Add(in NativeArray<float3> gridDim, ref MeshBuffersIndexers buffersIndexers, ref bool isStripStarted)
        { 
            Assert.IsTrue(!_prevIndices.IsCreated || _prevIndices.Length == gridDim.Length, "GridBuilder supports only gridDims of the same size");
            if (!isStripStarted)
            {
                isStripStarted = true;
                Start(gridDim, ref buffersIndexers);
            }
            else
            { 
                Continue(gridDim, ref buffersIndexers);
            }
        }

        /// <summary>
        /// Who knows if it will be needed. I do not.
        /// </summary>
        public void DisposeTempIndexing()
        {
            _prevIndices.Dispose();
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
            vertex.uv = _uv;
            
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