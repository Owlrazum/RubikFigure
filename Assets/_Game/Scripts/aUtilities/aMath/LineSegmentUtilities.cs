using Unity.Mathematics;

namespace Orazum.Math
{
    public static class LineSegmentUtilities
    { 
        public static float DistanceLineSegment(float3 p1, float3 p2)
        {
            return math.length(p2 - p1);
        }
        public static float DistanceLineSegment(in float3x2 lineSegment)
        {
            return DistanceLineSegment(lineSegment[0], lineSegment[1]);
        }

        public static float3 GetPerpDirection(quaternion q, float3 p1, float3 p2)
        { 
            float3 direction = math.normalize(p2 - p1);
            return math.rotate(q, direction);
        }
        public static float3 GetPerpDirection(quaternion q, in float3x2 lineSegment)
        {
            return GetPerpDirection(q, lineSegment[0], lineSegment[1]);
        }

        public static float3 GetLineSegmentCenter(float3 p1, float3 p2)
        {
            return (p2 - p1) / 2;
        }
        public static float3 GetLineSegmentCenter(in float3x2 lineSegment)
        {
            return GetLineSegmentCenter(lineSegment[0], lineSegment[1]);
        }
    }
}