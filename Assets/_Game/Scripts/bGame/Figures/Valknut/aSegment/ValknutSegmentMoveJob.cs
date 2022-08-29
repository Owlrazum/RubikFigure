using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;

using Orazum.Meshing;

// [BurstCompile]
public struct ValknutSegmentMoveJob : IJob
{
    public float P_LerpParam;

    public QSTransition InputQuadStripTransition;
    
    [WriteOnly]
    public NativeArray<MeshBuffersIndexers> OutputIndexers;

    public void Execute()
    {
        MeshBuffersIndexers indexers = new MeshBuffersIndexers();
        InputQuadStripTransition.UpdateWithLerpPos(P_LerpParam, ref indexers);
        OutputIndexers[0] = indexers;
    }
} 