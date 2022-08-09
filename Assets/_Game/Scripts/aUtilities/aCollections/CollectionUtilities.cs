using Unity.Collections;
using UnityEngine;

namespace Orazum.Collections
{
    public static class CollectionUtilities
    {
        public static NativeArray<T> GetSlice<T>(NativeArray<T> source, int start, int count) where T : struct
        {
            NativeArray<T> slice = new NativeArray<T>(count, Allocator.Persistent);
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

        public static void DisposeIfNeeded<T>(NativeArray<T> array) where T : struct
        { 
            if (array.IsCreated)
            {
                array.Dispose();
            }
        }
    }
}
