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

            QSTS_RadialBuilder.Parameters parameters = new()
            {
                LerpRange = WholeLerpRange,
                LerpLength = WholeLerpLength,
                IsTemporary = false,
                Construct = ConstructType.New,
            };

            parameters.Fill = clockOrder == ClockOrderType.CW ? FillType.FromStart : FillType.FromEnd;
            _radialBuilder.SingleRotationLerp(in origin, parameters, out QST_Segment qsts);
            parameters.Fill = clockOrder == ClockOrderType.CW ? FillType.ToEnd : FillType.ToStart;
            writeBuffer[QSTS_indexer++] = qsts;
            _radialBuilder.SingleRotationLerp(in target, parameters, out qsts);
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

            QSTS_RadialBuilder.Parameters parameters = new()
            {
                LerpRange = WholeLerpRange,
                LerpLength = WholeLerpLength,
                IsTemporary = false,
                Construct = ConstructType.New,
            };
            parameters.Fill = vertOrder == VertOrderType.Up ? FillType.ToEnd : FillType.ToStart;
            _radialBuilder.MoveLerp(origin, parameters, out QST_Segment s1);
            parameters.Fill = vertOrder == VertOrderType.Up ? FillType.FromStart : FillType.FromEnd;
            _radialBuilder.MoveLerp(target, parameters, out QST_Segment s2);
            writeBuffer[QSTS_indexer++] = s1;
            writeBuffer[QSTS_indexer++] = s2;
        }
    }
}