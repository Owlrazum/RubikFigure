using Unity.Mathematics;
using Unity.Collections;

public class ValknutVerticesMove : FigureSegmentMove
{
    public ValknutVerticesMove()
    {
        FromIndex = new int2(-1, -1);
        ToIndex = new int2(-1, -1);
    }

    public ValknutVerticesMove(FigureSegmentMove move)
    {
        FromIndex = move.FromIndex;
        ToIndex = move.ToIndex;
        LerpSpeed = move.LerpSpeed;
        Mover = move.Mover;
    }

    public NativeArray<float4x2>.ReadOnly TransitionPositions { get; private set; }
    public void AssignTransitionPositions(NativeArray<float4x2>.ReadOnly transitionPositions)
    {
        TransitionPositions = transitionPositions;
    }

    public NativeArray<float3>.ReadOnly LerpRanges { get; private set; }
    public void AssignTransitionLerpRanges(NativeArray<float3>.ReadOnly lerpRanges)
    {
        LerpRanges = lerpRanges;
    }
}
