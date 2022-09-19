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
        private NativeArray<int> _indexBuffer;
        public QuadGridBuilder(in NativeArray<VertexData> vertices, in NativeArray<short> indices, in float2 uv)
        {
            _vertices = vertices;
            _indices = indices;
            _uv = uv;
            _prevIndices = new NativeArray<int>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _indexBuffer = new NativeArray<int>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        public void Start(in NativeArray<float3> gridDim, ref MeshBuffersIndexers buffersIndexers)
        {
            _prevIndices = new NativeArray<int>(gridDim.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            _indexBuffer = new NativeArray<int>(gridDim.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < gridDim.Length; i++)
            {
                _prevIndices[i] = AddVertex(gridDim[i], ref buffersIndexers);
            }
        }

        public void Continue(in NativeArray<float3> gridDim, ref MeshBuffersIndexers buffersIndexers)
        {
            Assert.IsTrue(_prevIndices.Length == gridDim.Length, "GridBuilder supports only gridDims of the same size");
            Assert.IsTrue(_prevIndices.Length > 1, "Grid cannot consist from dimension equal or less than 1");

            int2 indexer = new int2(0, 1);
            int prevIndex = AddVertex(gridDim[0], ref buffersIndexers);
            _indexBuffer[0] = prevIndex;
            for (int i = 1; i < gridDim.Length; i++)
            {
                int newIndex = AddVertex(gridDim[i], ref buffersIndexers);
                _indexBuffer[i] = newIndex;
                int2 prevDimIndices = new int2(_prevIndices[indexer.x], _prevIndices[indexer.y]);
                int4 quadIndices = new int4(prevDimIndices, newIndex, prevIndex);
                AddQuadIndices(quadIndices, ref buffersIndexers);
                indexer++;
                prevIndex = newIndex;
            }

            for (int i = 0; i < _prevIndices.Length; i++)
            {
                _prevIndices[i] = _indexBuffer[i];
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

        private void AddQuadIndices(in int4 quadIndices, ref MeshBuffersIndexers buffersIndexers)
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