using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using Orazum.Meshing;

using static Orazum.Constants.Math;
using static Orazum.Math.LineSegmentUtilities;
using static QSTS_FillData;

public struct WheelTransitionsBuilder
{
    private QSTS_RadialBuilder _radialBuilder;
    private readonly float2 WholeLerpRange;
    private readonly float WholeLerpLength;

    public WheelTransitionsBuilder(int sideCount, int segmentResolution)
    {
        _radialBuilder = new();

        WholeLerpRange = new float2(0, 1);
        WholeLerpLength = 1;

        float2 anglesRad = new float2(TAU / sideCount, TAU / 2);
        float3 primaryAxis = math.up();
        _radialBuilder = new QSTS_RadialBuilder(primaryAxis, anglesRad, segmentResolution);
    }

    public void BuildClockOrderTransition(
        ref QuadStripsBuffer quadStrips,
        ref NativeArray<int2> originsTargets,
        ref NativeArray<QST_Segment> writeBuffer,
        ClockOrderType clockOrder
    )
    {
        int QSTS_indexer = 0;
        for (int i = 0; i < originsTargets.Length; i++)
        {
            int2 originTarget = originsTargets[i];
            QuadStrip origin = quadStrips.GetQuadStrip(originTarget.x);
            QuadStrip target = quadStrips.GetQuadStrip(originTarget.y);

            _radialBuilder.FillOut_SRL(in origin, WholeLerpRange, WholeLerpLength, isNew: true, clockOrder, out QST_Segment qsts);
            writeBuffer[QSTS_indexer++] = qsts;
            _radialBuilder.FillIn_SRL(in target, WholeLerpRange, WholeLerpLength, isNew: true, clockOrder, out qsts);
            writeBuffer[QSTS_indexer++] = qsts;
        }
    }

    public void BuildVertOrderTransition(
        ref QuadStripsBuffer quadStrips,
        ref NativeArray<int2> originsTargets,
        ref NativeArray<QST_Segment> writeBuffer,
        VertOrderType vertOrder
    )
    {
        int QSTS_indexer = 0;
        
        for (int i = 0; i < originsTargets.Length; i++)
        {
            int2 originTarget = originsTargets[i];
            QuadStrip origin = quadStrips.GetQuadStrip(originTarget.x);
            QuadStrip target = quadStrips.GetQuadStrip(originTarget.y);

            if (i == 0 && vertOrder == VertOrderType.Down)
            {
                GetLerpPointsForLevitation(origin, target, vertOrder, out float2 lerpRange, out float lerpLength);
                _radialBuilder.GenerateSingleMoveLerp(origin, new float2(0, lerpRange.x), WholeLerpLength, isNew: true, vertOrder, out QST_Segment s1);
                _radialBuilder.FillIn_DRL        (in origin, in target, lerpRange, lerpLength, isNew: true, vertOrder, out QST_Segment s2);
                _radialBuilder.GenerateSingleMoveLerp(target, new float2(lerpRange.y, 1), WholeLerpLength, isNew: true, vertOrder, out QST_Segment s3);
                writeBuffer[QSTS_indexer++] = s1;
                writeBuffer[QSTS_indexer++] = s2;
                writeBuffer[QSTS_indexer++] = s3;
            }
            else if (i == originsTargets.Length - 1 && vertOrder == VertOrderType.Up)
            {
                GetLerpPointsForLevitation(origin, target, vertOrder, out float2 lerpRange, out float lerpLength);
                _radialBuilder.GenerateSingleMoveLerp(origin, new float2(0, lerpRange.x), WholeLerpLength, isNew: true, vertOrder, out QST_Segment s1);
                _radialBuilder.FillIn_DRL        (in origin, in target, lerpRange, lerpLength, isNew: true, vertOrder, out QST_Segment s2);
                _radialBuilder.GenerateSingleMoveLerp(target, new float2(lerpRange.y, 1), WholeLerpLength, isNew: true, vertOrder, out QST_Segment s3);
                writeBuffer[QSTS_indexer++] = s1;
                writeBuffer[QSTS_indexer++] = s2;
                writeBuffer[QSTS_indexer++] = s3;
            }
            else
            {
                _radialBuilder.GenerateDoubleMoveLerp(in origin, in target, WholeLerpRange, WholeLerpLength, isNew: true, vertOrder, out QST_Segment qsts);
                writeBuffer[QSTS_indexer++] = qsts;
            }
        }
    }

    private void GetLerpPointsForLevitation(
        in QuadStrip origin,
        in QuadStrip target,
        VertOrderType vertOrder,
        out float2 lerpRange,
        out float lerpLength
    )
    {
        float moveDistance, levitationDistance, radius;
        if (vertOrder == VertOrderType.Down)
        {
            moveDistance = DistanceLineSegment(origin[0][0], origin[0][1]);
            radius = DistanceLineSegment(origin[0][0], target[0][1]) / 2;
        }
        else
        { 
            moveDistance = DistanceLineSegment(origin[0][0], origin[0][1]);
            radius = DistanceLineSegment(origin[0][1], target[0][0]) / 2;
        }
        levitationDistance = radius * TAU / 2;

        float totalDistance = moveDistance + levitationDistance + moveDistance;

        float start = moveDistance / totalDistance;
        float end = start + levitationDistance / totalDistance;
        lerpRange = new float2(start, end);
        lerpLength = moveDistance / totalDistance;
    }
}