using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using Orazum.Meshing;

// [BurstCompile]
public struct FigureSegmentMoveJob : IJob
{
    public float P_LerpParam;
    public bool P_ShouldReorientVertices;

    public QSTransitionAnimator InputQuadStripTransition;
    
    public MeshBuffersIndexersForJob OutputIndexers;

    public void Execute()
    {
        MeshBuffersIndexers indexers = OutputIndexers.GetIndexersForChangesInsideJob();;
        InputQuadStripTransition.UpdateWithLerpPos(P_LerpParam, P_ShouldReorientVertices, ref indexers);
        OutputIndexers.ApplyChanges(indexers);
    }
} 