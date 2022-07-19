using System;
using System.Collections;
using System.Collections.Generic;

namespace MarchingCubes
{ 
    /// <summary>
    /// A container for the corners of a voxel
    /// </summary>
    /// <typeparam name="T">The element's type to hold in the corners</typeparam>
    public struct VoxelCorners<T> : IEnumerable<T>
    {
        public T Corner1 { get; set; }
        public T Corner2 { get; set; }
        public T Corner3 { get; set; }
        public T Corner4 { get; set; }
        public T Corner5 { get; set; }
        public T Corner6 { get; set; }
        public T Corner7 { get; set; }
        public T Corner8 { get; set; }

        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Corner1;
                    case 1: return Corner2;
                    case 2: return Corner3;
                    case 3: return Corner4;
                    case 4: return Corner5;
                    case 5: return Corner6;
                    case 6: return Corner7;
                    case 7: return Corner8;
                    default: throw new ArgumentOutOfRangeException($"There are only 8 corners! You tried to access the corner at index {index}");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Corner1 = value;
                        break;
                    case 1:
                        Corner2 = value;
                        break;
                    case 2:
                        Corner3 = value;
                        break;
                    case 3:
                        Corner4 = value;
                        break;
                    case 4:
                        Corner5 = value;
                        break;
                    case 5:
                        Corner6 = value;
                        break;
                    case 6:
                        Corner7 = value;
                        break;
                    case 7:
                        Corner8 = value;
                        break;
                    default: throw new ArgumentOutOfRangeException($"There are only 8 corners! You tried to access the corner at index {index}");
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < 8; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}