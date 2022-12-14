using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine.Assertions;

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

        public T this[int index]
        {
            get { return _array[IndexUtilities.IndexToX(index, ColCount), IndexUtilities.IndexToY(index, ColCount)]; }
            set { _array[IndexUtilities.IndexToX(index, ColCount), IndexUtilities.IndexToY(index, ColCount)] = value; }
        }

        public ref T GetElementByRef(int2 index)
        {
            return ref _array[index.x, index.y];
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

        public void Swap(int2 lhs, int2 rhs)
        {
            T value = this[rhs];
            this[rhs] = this[lhs];
            this[lhs] = value;
        }

        public void RandomDerangement(int2 rowOrCol)
        {
            Assert.IsTrue(rowOrCol.x >= 0 && rowOrCol.y < 0 || rowOrCol.y >= 0 && rowOrCol.x < 0);
            if (rowOrCol.x >= 0)
            {
                T[] row = new T[ColCount];
                for (int i = 0; i < ColCount; i++)
                {
                    row[i] = _array[i, rowOrCol.x];
                }
                Algorithms.RandomDerangement(row);
                for (int i = 0; i < ColCount; i++)
                {
                    _array[i, rowOrCol.x] = row[i];
                }
            }
            else
            { 
                T[] col = new T[RowCount];
                for (int i = 0; i < RowCount; i++)
                {
                    col[i] = _array[rowOrCol.y, i];
                }
                Algorithms.RandomDerangement(col);
                for (int i = 0; i < RowCount; i++)
                {
                    _array[rowOrCol.y, i] = col[i];
                }
            }
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



