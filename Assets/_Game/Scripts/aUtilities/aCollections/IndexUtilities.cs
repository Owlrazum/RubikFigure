using Unity.Mathematics;

namespace Orazum.Collections
{
    /// <summary>
    /// Blender style coordinate system. Construct row x axis, construct column z axis, construct heith
    /// </summary>
    public static class IndexUtilities
    {
        /// <summary>
        /// Get random integer within range that is not equal to the toExclude
        /// </summary>
        public static int RandomRangeWithExlusion(int start, int end, int toExclude)
        {
            int result = UnityEngine.Random.Range(start, end - 1);
            if (result >= toExclude)
            {
                result++;
            }
            return result;
        }

        public static int2 IndexToXy(int index, int columnCount)
        {
            return new int2(index % columnCount, index / columnCount);
        }

        public static int IndexToX(int index, int columnCount)
        {
            return index % columnCount;
        }

        public static int IndexToY(int index, int columnCount)
        {
            return index / columnCount;
        }

        public static int XyToIndex(int2 xy, int columnCount)
        {
            return xy.y * columnCount + xy.x;
        }

        public static int XyToIndex(int x, int y, int columnCount)
        {
            return y * columnCount + x;
        }

        public static int3 IndexToXyz(int index, int columnCount, int rowCount)
        {
            int3 position = new int3(
                index % columnCount,
                index / (columnCount * rowCount),
                index / columnCount % rowCount
            );
            return position;
        }

        public static int XyzToIndex(int3 xyz, int columnCount, int rowCount)
        {
            return xyz.y * columnCount * rowCount + xyz.z * columnCount + xyz.x;
        }

        public static int XyzToIndex(int x, int y, int z, int columnCount, int rowCount)
        {
            return y * columnCount * rowCount + z * columnCount + x;
        }

        public static int XyzToX(int xyz, int columnCount)
        {
            return xyz % columnCount;
        }

        public static int XyzToY(int xyz, int columnCount, int rowCount)
        {
            return xyz / (columnCount * rowCount);
        }

        public static int XyzToZ(int xyz, int columnCount, int rowCount)
        {
            return xyz / columnCount % rowCount;
        }

        public static void IncreaseIndex(ref int index, int count)
        {
            if (index + 1 >= count)
            {
                index = 0;
            }
            else
            {
                index++;
            }
        }

        public static int IncreaseIndex(int index, int count)
        { 
            if (index + 1 >= count)
            {
                return 0;
            }
            else
            {
                return index + 1;
            }
        }

        public static void DecreaseIndex(ref int index, int count)
        {
            if (index - 1 < 0)
            {
                index = count - 1;
            }
            else
            {
                index--;
            }
        }

        public static int DecreaseIndex(int index, int count)
        { 
            if (index - 1 < 0)
            {
                return count - 1;
            }
            else
            {
                return index - 1;
            }
        }
    }
}