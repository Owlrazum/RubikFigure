using NUnit.Framework;

using Unity.Collections;
using Orazum.Utilities;

public class TestCollectionUtils
{
    [Test]
    public void Reverse()
    {
        int evenCount = 10;
        using (NativeArray<int> toReverseEven = InitializeNativeArray(evenCount))
        { 
            CollectionUtilities.ReverseNativeArray(toReverseEven);
            for (int i = 0; i < evenCount; i++)
            {
                Assert.AreEqual(evenCount - 1 - i, toReverseEven[i]);
            }
        }

        int oddCount = 7;
        using (NativeArray<int> toReverseOdd = InitializeNativeArray(oddCount))
        { 
            CollectionUtilities.ReverseNativeArray(toReverseOdd);
            for (int i = 0; i < oddCount; i++)
            {
                Assert.AreEqual(oddCount - 1 - i, toReverseOdd[i]);
            }
        }
    }

    [Test]
    public void GetSlice()
    {
        int totalCount = 10;
        using (NativeArray<int> total = InitializeNativeArray(totalCount))
        {
            using (NativeArray<int> slice_1 = CollectionUtilities.GetSlice(total, 0, 3))
            {
                for (int i = 0; i < 3; i++)
                {
                    Assert.AreEqual(i, slice_1[i]);
                }
            }

            using (NativeArray<int> slice_2 = CollectionUtilities.GetSlice(total, 2, 7))
            {
                for (int i = 0; i < 7; i++)
                {
                    Assert.AreEqual(i + 2, slice_2[i]);
                }
            }
        }

        totalCount = 5;
        using (NativeArray<int> total = InitializeNativeArray(totalCount))
        { 
            using (NativeArray<int> slice_1 = CollectionUtilities.GetSlice(total, 3, 2))
            {
                for (int i = 0; i < 2; i++)
                {
                    Assert.AreEqual(i + 3, slice_1[i]);
                }
            }

            using (NativeArray<int> slice_2 = CollectionUtilities.GetSlice(total, 0, 5))
            {
                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(i, slice_2[i]);
                }
            }
        }
    }

    private NativeArray<int> InitializeNativeArray(int itemCount)
    { 
        NativeArray<int> toReturn = new NativeArray<int>(itemCount, Allocator.Temp);
        for (int i = 0; i < itemCount; i++)
        {
            toReturn[i] = i;
        }
        return toReturn;
    }
}
