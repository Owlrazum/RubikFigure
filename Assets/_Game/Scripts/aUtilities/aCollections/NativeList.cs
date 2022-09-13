using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Orazum.Collections
{ 
    /// <summary>
    /// Non extendable quickly written native list using native array 
    /// </summary>
    public struct NativeList<T> : IDisposable where T : struct
    {
        private NativeArray<T> _array;
        public NativeList(int maxCount, Allocator allocator)
        {
            _array = new(maxCount, allocator);
            Count = 0;
        }

        public int Count { get; private set; }

        public T this[int index]
        {
            get 
            {
                if (index >= Count)
                { 
                    throw new ArgumentOutOfRangeException("Index is out of Count property");
                }
                return _array[index];
            }
            set
            {
                if (index >= Count)
                { 
                    throw new ArgumentOutOfRangeException("Index is out of Count property");
                }
                _array[index] = value;
            }
        }

        public void Add(in T value)
        {
            _array[Count++] = value;
        }

        public void Clear()
        {
            Count = 0;
        }

        public void Dispose()
        {
            _array.Dispose();
        }

        public void DisposeIfNeeded()
        {
            CollectionUtilities.DisposeIfNeeded(_array);
        }
    }
}