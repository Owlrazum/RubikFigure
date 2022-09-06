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
    private float2 WholeLerpRange;

    public WheelTransitionsBuilder(int sideCount, int segmentResolution)
    {
        _radialBuilder = new();
        _radialBuilder.Resolution = segmentResolution;

        WholeLerpRange = new float2(0, 1);

        float rotationAngle = TAU / sideCount;
        float4 axisAngleCW = new float4(math.up(), rotationAngle);
        float4 axisAngleAntiCW = new float4(math.up(), -rotationAngle);
        _radialBuilder.SRL_AxisAngleCW = axisAngleCW;
        _radialBuilder.SRL_AxisAngleAntiCW = axisAngleAntiCW;
    }

    public void BuildClockOrderTransition(
        ref QuadStripsBuffer quadStrips,
        ref NativeArray<int2> originsTargets,
        ref NativeArray<QST_Segment> writeBuffer,
        ClockOrderType clockOrder
    )
    {
        int QSTS_indexer = 0;
        _radialBuilder.ClockOrder = clockOrder;

        for (int i = 0; i < originsTargets.Length; i++)
        {
            int2 originTarget = originsTargets[i];
            QuadStrip origin = quadStrips.GetQuadStrip(originTarget.x);
            QuadStrip target = quadStrips.GetQuadStrip(originTarget.y);

            _radialBuilder.FillOut_SRL(in origin, WholeLerpRange, FillType.NewToEnd, out QST_Segment qsts);
            writeBuffer[QSTS_indexer++] = qsts;
            _radialBuilder.FillIn_SRL(in target, WholeLerpRange, FillType.ContinueFromStart, out qsts);
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
        _radialBuilder.VertOrder = vertOrder;

        int QSTS_indexer = 0;
        
        for (int i = 0; i < originsTargets.Length; i++)
        {
            int2 originTarget = originsTargets[i];
            QuadStrip origin = quadStrips.GetQuadStrip(originTarget.x);
            QuadStrip target = quadStrips.GetQuadStrip(originTarget.y);

            if (i == 0 && vertOrder == VertOrderType.Down)
            {
                float2 lerpPoints = GetLerpPointsForLevitation(origin, target, vertOrder);
                _radialBuilder.GenerateMoveLerp(origin, new float2(0, lerpPoints.x), out QST_Segment s1);
                _radialBuilder.GenerateDoubleRotationLerp(in origin, in target, out QST_Segment s2);
                _radialBuilder.GenerateMoveLerp(target, new float2(lerpPoints.y, 1), out QST_Segment s3);
                writeBuffer[QSTS_indexer++] = s1;
                writeBuffer[QSTS_indexer++] = s2;
                writeBuffer[QSTS_indexer++] = s3;
            }
            else if (i == originsTargets.Length - 1 && vertOrder == VertOrderType.Up)
            {
                float2 lerpPoints = GetLerpPointsForLevitation(origin, target, vertOrder);
                _radialBuilder.GenerateMoveLerp(origin, new float2(0, lerpPoints.x), out QST_Segment s1);
                _radialBuilder.GenerateDoubleRotationLerp(in origin, in target, out QST_Segment s2);
                _radialBuilder.GenerateMoveLerp(target, new float2(lerpPoints.y, 1), out QST_Segment s3);
                writeBuffer[QSTS_indexer++] = s1;
                writeBuffer[QSTS_indexer++] = s2;
                writeBuffer[QSTS_indexer++] = s3;
            }
            else
            {
                _radialBuilder.GenerateMoveLerpWithMiddle(in origin, in target, WholeLerpRange, out QST_Segment qsts);
                writeBuffer[QSTS_indexer++] = qsts;
            }
        }
    }

    private float2 GetLerpPointsForLevitation(
        in QuadStrip origin,
        in QuadStrip target,
        VertOrderType vertOrder
    )
    {
        float d1 = 0, d2 = 0;
        if (vertOrder == VertOrderType.Down)
        {
            d1 = DistanceLineSegment(origin[0][0], origin[0][1]);
            d2 = DistanceLineSegment(origin[0][0], target[0][1]);
        }
        else if (vertOrder == VertOrderType.Up)
        { 
            d1 = DistanceLineSegment(origin[0][0], origin[0][1]);
            d2 = DistanceLineSegment(origin[0][1], target[0][0]);
        }

        float total = d1 + d2 + d1;
        float2 lerpPoints = new float2(d1 / total, 0);
        lerpPoints.y = lerpPoints.y + d2 / total;
        return lerpPoints;
    }

}