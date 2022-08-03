using Unity.Collections;
using UnityEngine;

namespace Orazum.Utilities
{
    public static class CollectionsUtilities
    {
        public static NativeArray<VertexData> GetVerticesSlice(NativeArray<VertexData> source, int start, int count)
        {
            NativeArray<VertexData> slice = new NativeArray<VertexData>(count, Allocator.Persistent);
            for (int i = 0; i < count; i++)
            {
                slice[i] = source[i + start];
            }
            return slice;
        }

        public static void ReverseNativeArray<T>(NativeArray<T> array) where T : struct
        {
            int start = 0;
            int end = array.Length - 1;
            for (int i = 0; i < array.Length; i++)
            {
                T element = array[start];
                array[start] = array[end];
                array[end] = element;
                start++;
                end--;
                if (start >= end)
                {
                    break;
                }
            }
        }
    }
}
