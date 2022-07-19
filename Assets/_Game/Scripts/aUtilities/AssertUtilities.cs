using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Orazum.Utilities
{ 
    public static class AssertUtilities
    {
        public static void AreApproximatelyEqual(float3 a, float3 b)
        { 
            Assert.AreApproximatelyEqual(a.x, b.x);
            Assert.AreApproximatelyEqual(a.y, b.y);
            Assert.AreApproximatelyEqual(a.z, b.z);
        }
    }
}