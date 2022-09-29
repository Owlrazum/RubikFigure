using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using Orazum.Meshing;

// [BurstCompile]
public struct FigureSegmentMoveJob : IJob
{
    public float P_LerpParam;

    public QST_Animator InputQuadStripTransition;
    
    public MeshBuffersIndexersForJob OutputIndexers;

    public void Execute()
    {
        MeshBuffersIndexers indexers = OutputIndexers.GetIndexersForChangesInsideJob();;
        InputQuadStripTransition.UpdateWithLerpPos(P_LerpParam, ref indexers);
        OutputIndexers.ApplyChanges(indexers);
    }
} 