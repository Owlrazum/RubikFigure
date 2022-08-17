using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using static Orazum.Math.MathUtilities;

public class SegmentPointVertexPositionsTests
{
    [Test]
    public void IndexTests()
    {
        TestIndices(5);
        TestIndices(10);
    }

    private void TestIndices(int resolution)
    { 
        float4 mockData = new float4(1, 2, TAU / 3, 1 / resolution);
        SegmentMesh mock = new SegmentMesh(new float3(0, 0, 1), mockData, resolution);

        for (int i = 0; i < (resolution + 1) * 2; i += 2)
        {
            if (i < 2)
            { 
                Assert.AreEqual(new int2(0, -1), mock.GetSegmentIndices(i));
                Assert.AreEqual(new int2(1, -1), mock.GetSegmentIndices(i + 1));
                continue;
            }

            int j = i * 2;
            if (j >= resolution * 4 - 2)
            {
                Assert.AreEqual(new int2(j - 1, -1), mock.GetSegmentIndices(i));
                Assert.AreEqual(new int2(j - 2, -1), mock.GetSegmentIndices(i + 1));
                continue;
            }

            Assert.AreEqual(new int2(j - 1, j), mock.GetSegmentIndices(i));
            Assert.AreEqual(new int2(j - 2, j + 1), mock.GetSegmentIndices(i + 1));        
        }
    }
}

/*
    Assert.AreEqual(new int2(0, -1), mock.GetSegmentIndices(0));
    Assert.AreEqual(new int2(1, -1), mock.GetSegmentIndices(1));
    
    Assert.AreEqual(new int2(3, 4), mock.GetSegmentIndices(2));
    Assert.AreEqual(new int2(2, 5), mock.GetSegmentIndices(3));
    
    Assert.AreEqual(new int2(7, 8), mock.GetSegmentIndices(4));
    Assert.AreEqual(new int2(6, 9), mock.GetSegmentIndices(5));
    
    Assert.AreEqual(new int2(11, 12), mock.GetSegmentIndices(6));
    Assert.AreEqual(new int2(10, 13), mock.GetSegmentIndices(7));
    
    Assert.AreEqual(new int2(15, 16), mock.GetSegmentIndices(8));
    Assert.AreEqual(new int2(14, 17), mock.GetSegmentIndices(9));
    
    Assert.AreEqual(new int2(19, -1), mock.GetSegmentIndices(10));
    Assert.AreEqual(new int2(18, -1), mock.GetSegmentIndices(11));
*/
