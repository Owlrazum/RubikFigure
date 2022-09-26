using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using Orazum.Math;
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


        bool samePart = originPart == targetPart;
        LineEndDirectionType originDirection = samePart ? LineEndDirectionType.StartToEnd : LineEndDirectionType.EndToStart;
        dataBuilder.InitializeOriginRays(
            samePart ? LineEndType.End : LineEndType.Start,
            samePart ? LineEndDirectionType.StartToEnd : LineEndDirectionType.EndToStart
        );

        LineEndDirectionType targetDirection = LineEndDirectionType.StartToEnd;
        if (originPart == 0 && targetPart == 0)
        { 
            dataBuilder.InitializeTargetRay(LineEndType.Start, LineEndDirectionType.EndToStart, LineEndType.End);
        }
        else if (originPart == 0 && targetPart == 1)
        { 
            dataBuilder.InitializeTargetRay(LineEndType.End, LineEndDirectionType.StartToEnd, LineEndType.End);
        }
        else if (originPart == 1 && targetPart == 0)
        { 
            dataBuilder.InitializeTargetRay(LineEndType.Start, LineEndDirectionType.EndToStart, LineEndType.Start);
        }
        else
        { 
            dataBuilder.InitializeTargetRay(LineEndType.End, LineEndDirectionType.StartToEnd, LineEndType.Start);
        }

        dataBuilder.BuildTransition(originDirection, targetDirection, ref writeBuffer);
    }
}

/*
    TasToTas
        originRays = _origin.GetRays(LineEndType.End, LineEndDirectionType.StartToEnd);
        targetRay = _target.GetRay(LineEndType.Start, LineEndType.End, LineEndDirectionType.EndToStart);
    TasToOas:
        originRays = _origin.GetRays(LineEndType.Start, LineEndDirectionType.EndToStart);
        targetRay = _target.GetRay(LineEndType.End, LineEndType.End, LineEndDirectionType.StartToEnd);
    OasToTas:
        originRays = _origin.GetRays(LineEndType.Start, LineEndDirectionType.EndToStart);
        targetRay = _target.GetRay(LineEndType.Start, LineEndType.Start, LineEndDirectionType.EndToStart);
    OasToOas:
        originRays = _origin.GetRays(LineEndType.End, LineEndDirectionType.StartToEnd);
        targetRay = _target.GetRay(LineEndType.End, LineEndType.Start, LineEndDirectionType.StartToEnd);
*/