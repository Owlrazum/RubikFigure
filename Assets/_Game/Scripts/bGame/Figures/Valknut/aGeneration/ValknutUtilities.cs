using Unity.Mathematics;
using Unity.Collections;

using Orazum.Math;
using Orazum.Meshing;
using static Orazum.Math.MathUtilities;

public static class ValknutUtilities
{
    public static void BuildTransitionData(
        in ValknutSegmentMesh origin,
        in ValknutSegmentMesh target,
        ClockOrderType clockOrder,
        ref NativeArray<float4x2> transitionPositions,
        ref NativeArray<float3> lerpRanges
        )
    {
        int firstHalfRangesCount = origin.StripSegmentsCount - 1;
        int secondHalfRangesCount = target.StripSegmentsCount - 1;
        int2 rangesCount = new int2(0, firstHalfRangesCount + secondHalfRangesCount);

        float4x2 targetRays = target.GetRays(clockOrder, DirectionOrderType.Start);
        float4x2 originRays = origin.GetRays(clockOrder, DirectionOrderType.End);
        IntersectSegmentRays(originRays, targetRays, out float2x2 intersectSegment);

        NativeArray<float> distancesSq = new NativeArray<float>(8, Allocator.Temp);
        float totalDistanceSq = 0;
        float2x2 delta = float2x2.zero;
        float4x2 transitionPosition = float4x2.zero;

        if (clockOrder == ClockOrderType.CW)
        {
            for (int i = 0; i < firstHalfRangesCount; i++)
            {
                if (i == firstHalfRangesCount - 1)
                {
                    delta = intersectSegment - origin[i];
                    transitionPosition[0] = new float4(origin[i][0], origin[i][1]);
                    transitionPosition[1] = new float4(intersectSegment[0], intersectSegment[1]);
                }
                else
                {
                    delta = origin[i + 1] - origin[i];
                    transitionPosition[0] = new float4(origin[i][0], origin[i][1]);
                    transitionPosition[0] = new float4(origin[i + 1][0], origin[i + 1][1]);
                }

                distancesSq[rangesCount.x] = math.lengthsq(delta[0]);
                totalDistanceSq += distancesSq[rangesCount.x];

                transitionPositions[rangesCount.x++] = transitionPosition;
            }
        }
        else
        {
            for (int i = firstHalfRangesCount - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    delta = intersectSegment - origin[i];
                    transitionPosition[0] = new float4(origin[i][0], origin[i][1]);
                    transitionPosition[1] = new float4(intersectSegment[0], intersectSegment[1]);
                }
                else
                {
                    delta = origin[i - 1] - origin[i];
                    transitionPosition[0] = new float4(origin[i][0], origin[i][1]);
                    transitionPosition[0] = new float4(origin[i - 1][0], origin[i - 1][1]);
                }

                distancesSq[rangesCount.x] = math.lengthsq(delta[0]);
                totalDistanceSq += distancesSq[rangesCount.x];

                transitionPositions[rangesCount.x++] = transitionPosition;
            }
        }

        for (int i = 0; i < secondHalfRangesCount; i++)
        {
            delta = target[i + 1] - target[i];
            distancesSq[rangesCount.x] = math.lengthsq(delta[0]);
            totalDistanceSq += distancesSq[rangesCount.x];

            transitionPosition[0] = new float4(target[i][0], target[i][1]);
            transitionPosition[1] = new float4(target[i + 1][0], target[i + 1][1]);
            transitionPositions[rangesCount.x++] = transitionPosition;
        }

        using (distancesSq)
        { 
            rangesCount.x = 0;
            float startLerpRange = 0;
            float nextLerpRange = distancesSq[0] / totalDistanceSq;
            for (int i = 1; i <= rangesCount.y; i++)
            {
                float2 lerpRange = new float2(startLerpRange, nextLerpRange);
                float fillTypeFloat;
                if (i == 1)
                {
                    fillTypeFloat = QuadStripTransition.FillTypeToFloat(LerpRangeFillType.StartFill);
                }
                else
                {
                    fillTypeFloat = QuadStripTransition.FillTypeToFloat(LerpRangeFillType.UsualFill);
                }

                lerpRanges[rangesCount.x++] = new float3(lerpRange, fillTypeFloat);
                if (rangesCount.x > rangesCount.y)
                {
                    break;
                }
                startLerpRange = nextLerpRange;
                nextLerpRange = distancesSq[i] / totalDistanceSq;
            }
        }
    }
}