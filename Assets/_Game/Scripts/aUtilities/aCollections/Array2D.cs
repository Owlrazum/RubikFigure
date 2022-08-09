using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine.Assertions;

using Orazum.Utilities;

namespace Orazum.Collections
{
    public class Array2D<T> : IEnumerable<T>
    {
        private T[,] _array;
        private int2 _gridSize;

        public Array2D(int2 gridSizeArg)
        {
            _gridSize = gridSizeArg;
            _array = new T[_gridSize.x, _gridSize.y];
        }

        public Array2D(int colCount, int rowCount)
        {
            _gridSize = new int2(colCount, rowCount);
            _array = new T[_gridSize.x, _gridSize.y];
        }

        public int RowCount { get { return _gridSize.y; } }
        public int ColCount { get { return _gridSize.x; } }

        public T this[int2 index]
        {
            get { return _array[index.x, index.y]; }
            set { _array[index.x, index.y] = value; }
        }

        public T this[int col, int row]
        {
            get { return _array[col, row]; }
            set { _array[col, row] = value; }
        }

        public int GetIndex1D(int2 index)
        {
            Assert.IsTrue(index.x >= 0 && index.x < _gridSize.x && index.y >= 0 && index.y < _gridSize.y);
            return IndexUtilities.XyToIndex(index, _gridSize.x);
        }

        public int GetIndex1D(int col, int row)
        {
            Assert.IsTrue(col >= 0 && col < _gridSize.x && row >= 0 && row < _gridSize.y);
            return IndexUtilities.XyToIndex(col, row, _gridSize.x);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int row = 0; row < _gridSize.y; row++)
            {
                for (int col = 0; col < _gridSize.x; col++)
                {
                    yield return _array[col, row];
                }
            }
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            string log = "";
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColCount; j++)
                {
                    log += _array[j, i] + " ";
                }
                log += "\n";
            }

            return log.ToString();
        }
    }
}



