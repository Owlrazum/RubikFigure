using System;
using Unity.Collections;
using Unity.Mathematics;

using Orazum.Utilities;

namespace MarchingCubes
{ 
    /// <summary>
    /// A 3-dimensional field of data
    /// </summary>
    public struct ScalarField<T> : IDisposable where T : struct
    {
        private NativeArray<T> _data;

        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }

        public int Length => Width * Height * Depth;

        public ScalarField(int widthArg, int heightArg, int depthArg, Allocator allocator)
        {
    #if UNITY_EDITOR
            if (widthArg < 0 || heightArg < 0 || depthArg < 0)
            {
                throw new ArgumentException("The dimensions of this scalarField must all be positive!");
            }
    #endif

            _data = new NativeArray<T>(widthArg * heightArg * depthArg, allocator);

            Width = widthArg;
            Height = heightArg;
            Depth = depthArg;
        }

        public void Dispose()
        {
            _data.Dispose();
        }

        public void SetData(T data, int index)
        {
            _data[index] = data;
        }

        public bool TryGetData(int3 localPosition, out T data)
        {
            int index = IndexUtilities.XyzToIndex(localPosition, Width, Depth);
            if (index >= 0 && index < _data.Length)
            {
                data = GetData(index);
                return true;
            }
            data = default;
            return false;
        }

        public T GetData(int index)
        {
            return _data[index];
        }

        public int GetHeightIndex(int localPos)
        {
            return IndexUtilities.XyzToY(localPos, Width, Depth);
        }

        public int GetDistanceIndex(int localPos)
        {
            int x = IndexUtilities.XyzToX(localPos, Width);
            int y = IndexUtilities.XyzToZ(localPos, Width, Depth);
            return IndexUtilities.XyToIndex(new int2(x, y), Width);
        }

        public int GetDistanceIndex(int2 xy)
        {
            return IndexUtilities.XyToIndex(xy, Width);
        }
    }
}