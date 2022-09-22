using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Math;
using Orazum.Meshing;

using static Orazum.Constants.Math;
using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Collections.IndexUtilities;
using static Orazum.Meshing.QSTS_FillData;

public struct WheelTransitionsBuilder
{
    private QSTS_RadialBuilder _radialBuilder;
    private int2 _sidesRingsCount;

    public WheelTransitionsBuilder(int2 sidesRingsCount, int segmentResolution)
    {
        _radialBuilder = new();
        _sidesRingsCount = sidesRingsCount;

        float2 anglesRad = new float2(TAU / sidesRingsCount.x, TAU / 2);
        float3 primaryAxis = math.up();
        _radialBuilder = new QSTS_RadialBuilder(primaryAxis, anglesRad, segmentResolution);
    }

    public void BuildVertOrderTransition(
        ref QuadStripsBuffer quadStrips,
        NativeArray<QST_Segment> downBuffer,
        NativeArray<QST_Segment> upBuffer,
        int sideIndex
    )
    {
        int2 down_OT = new int2(0, 1);
        int2 up_OT = new int2(_sidesRingsCount.y - 1, 0);

        int downIndexer = 0;
        int upIndexer = 0;

        var parameters = QSTS_RadialBuilder.DefaultParameters();
        for (int ringIndexing = 0; ringIndexing < _sidesRingsCount.y; ringIndexing++)
        {
            int firstIndex = GetIndex(sideIndex, down_OT.x);
            int secondIndex = GetIndex(sideIndex, down_OT.y);
            QuadStrip first = quadStrips.GetQuadStrip(firstIndex);
            QuadStrip second = quadStrips.GetQuadStrip(secondIndex);

            parameters.Fill = FillType.FromEnd;
            _radialBuilder.MoveLerp(first, parameters, out QST_Segment qsts);
            downBuffer[downIndexer++] = qsts;

            parameters.Fill = FillType.ToStart;
            _radialBuilder.MoveLerp(second, parameters, out qsts);
            downBuffer[downIndexer++] = qsts;

            firstIndex = GetIndex(sideIndex, up_OT.x);
            secondIndex = GetIndex(sideIndex, up_OT.y);
            first = quadStrips.GetQuadStrip(firstIndex);
            second = quadStrips.GetQuadStrip(secondIndex);

            parameters.Fill = FillType.ToEnd;
            _radialBuilder.MoveLerp(first, parameters, out qsts);
            upBuffer[upIndexer++] = qsts;

            parameters.Fill = FillType.FromStart;
            _radialBuilder.MoveLerp(second, parameters, out qsts);
            upBuffer[upIndexer++] = qsts;

            IncreaseRing(ref down_OT.x);
            IncreaseRing(ref down_OT.y);
            IncreaseRing(ref up_OT.x);
            IncreaseRing(ref up_OT.y);
        }
    }

    public void BuildClockOrderTransition(
        ref QuadStripsBuffer quadStrips,
        NativeArray<QST_Segment> antiCwBuffer,
        NativeArray<QST_Segment> cwBuffer,
        int ringIndex
    )
    {
        int2 antiCW_OT = new int2(0, 1);
        int2 CW_OT = new int2(_sidesRingsCount.x - 1, 0);

        int antiCwIndexer = 0;
        int cwIndexer = 0;

        var parameters = QSTS_RadialBuilder.DefaultParameters();
        for (int sideIndexing = 0; sideIndexing < _sidesRingsCount.x; sideIndexing++)
        {
            int originIndex = GetIndex(antiCW_OT.x, ringIndex);
            int targetIndex = GetIndex(antiCW_OT.y, ringIndex);
            QuadStrip origin = quadStrips.GetQuadStrip(originIndex);
            QuadStrip target = quadStrips.GetQuadStrip(targetIndex);

            parameters.Fill = FillType.FromEnd;
            _radialBuilder.SingleRotationLerp(origin, parameters, out QST_Segment qsts);
            antiCwBuffer[antiCwIndexer++] = qsts;

            parameters.Fill = FillType.ToStart;
            _radialBuilder.SingleRotationLerp(target, parameters, out qsts);
            antiCwBuffer[antiCwIndexer++] = qsts;


            originIndex = GetIndex(CW_OT.x, ringIndex);
            targetIndex = GetIndex(CW_OT.y, ringIndex);
            origin = quadStrips.GetQuadStrip(originIndex);
            target = quadStrips.GetQuadStrip(targetIndex);

            parameters.Fill = FillType.ToEnd;
            _radialBuilder.SingleRotationLerp(origin, parameters, out qsts);
            cwBuffer[cwIndexer++] = qsts;

            parameters.Fill = FillType.FromStart;
            _radialBuilder.SingleRotationLerp(target, parameters, out qsts);
            cwBuffer[cwIndexer++] = qsts;

            IncreaseSide(ref antiCW_OT.x);
            IncreaseSide(ref antiCW_OT.y);
            IncreaseSide(ref CW_OT.x);
            IncreaseSide(ref CW_OT.y);
        }
    }

    private int GetIndex(int side, int ring)
    {
        return XyToIndex(ring, side, _sidesRingsCount.y);
    }

    private void IncreaseRing(ref int ring)
    {
        IncreaseIndex(ref ring, _sidesRingsCount.y);
    }

    private void IncreaseSide(ref int side)
    {
        IncreaseIndex(ref side, _sidesRingsCount.x);
    }
}