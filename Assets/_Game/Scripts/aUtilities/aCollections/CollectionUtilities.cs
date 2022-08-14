using System.Collections.Generic;
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

        public static void Swap<T>(this IList<T> list, int lhs, int rhs)
        {
            T element = list[lhs];
            list[lhs] = list[rhs];
            list[rhs] = element;
        }

        public static float SubFactorial(int n)
        {
            if (n < 2)
            {
                return 1;
            }
            return (float)( (Factorial(n) + 1) / System.Math.E);
        }

        public static int Factorial(int n)
        {
            if (n < 2)
            {
                return 1;
            }
            if (n == 2)
            {
                return 2;
            }
            return n * Factorial(n - 1);
        }

        public static int SubFactorialRecursive(int n, int recDepth = 0)
        {
            if (n < 0 || recDepth > 10000)
            {
                throw new System.ArgumentException("SubFactorial n negative");
            }
            if (n == 0)
            {
                return 1;
            }
            if (n == 1)
            {
                return 0;
            }
            return (n - 1) * (SubFactorialRecursive(n - 2, recDepth + 1) + SubFactorialRecursive(n - 1, recDepth + 1));
        }
    }
}
