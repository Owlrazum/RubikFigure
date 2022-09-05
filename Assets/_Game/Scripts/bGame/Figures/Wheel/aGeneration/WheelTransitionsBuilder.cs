using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using Orazum.Meshing;

using static Orazum.Constants.Math;
using static QSTS_FillData;

public struct WheelTransitionsBuilder
{
    private QSTS_RadialBuilder _radialBuilder;

    public WheelTransitionsBuilder(int sideCount, int segmentResolution)
    {
        _radialBuilder = new();
        _radialBuilder.Resolution = segmentResolution;
        
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

            _radialBuilder.FillOut(in origin, FillType.NewToEnd, out QST_Segment qsts);
            writeBuffer[QSTS_indexer++] = qsts;
            _radialBuilder.FillIn(in target, FillType.ContinueFromStart, out qsts);
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

            }
            else if (i == originsTargets.Length - 1 && vertOrder == VertOrderType.Up)
            {
            }
            else
            {
                _radialBuilder.GenerateMoveLerp(in origin, in target, out QST_Segment qsts);
                writeBuffer[QSTS_indexer++] = qsts;
            }
        }
    }

}