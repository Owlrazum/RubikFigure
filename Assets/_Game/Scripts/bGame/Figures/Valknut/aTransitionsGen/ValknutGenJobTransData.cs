using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using Orazum.Meshing;
using static Orazum.Collections.IndexUtilities;

// [BurstCompile]
public struct ValknutGenJobTransData : IJobFor
{
    [ReadOnly]
    public QuadStripsBuffer InQuadStripsCollection;

    public QS_TransitionsBuffer OutTransitionsCollection;

    private const int PartsCount = 2;
    private const int TrianglesCount = 3;

    public void Execute(int transitionIndex)
    {
        int targetTriangle = transitionIndex / 4;
        int originTriangle = DecreaseIndex(targetTriangle, TrianglesCount);

        int transitionType = transitionIndex % 4;
        int originPart = transitionType % 2;
        if (transitionType > 1)
        {
            originPart = transitionType == 2 ? 1 : 0;
        }
        int targetPart = transitionType / 2;

        int2 originTarget = new int2(
            XyToIndex(originPart, originTriangle, PartsCount),
            XyToIndex(targetPart, targetTriangle, PartsCount)
        );

        QuadStrip origin = InQuadStripsCollection.GetQuadStrip(originTarget.x);
        QuadStrip target = InQuadStripsCollection.GetQuadStrip(originTarget.y);

        ValknutTransitionsBuilder dataBuilder = new ValknutTransitionsBuilder(
            origin,
            target
        );

        NativeArray<QST_Segment> writeBuffer =
            OutTransitionsCollection.GetBufferSegment(transitionIndex);

        dataBuilder.BuildTransition(ref writeBuffer);
    }
}