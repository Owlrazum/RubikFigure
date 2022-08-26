using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

using Orazum.Meshing;

// [BurstCompile]
public struct ValknutSegmentMoveJob : IJob
{
    public float P_LerpParam;
    public QSTransition InputQuadStripTransition;
    public MeshBuffersIndexers BuffersData;

    public ValknutSegmentMoveJob(ref MeshBuffersIndexers buffersData, ref QSTransition quadStripTransition)
    {
        P_LerpParam = 0;
        BuffersData = buffersData;
        InputQuadStripTransition = quadStripTransition;
    }

    public void Execute()
    {
        Debug.Log("Executing");
        // BuffersData.Count = int2.zero;
        // BuffersData.Start = int2.zero;
        InputQuadStripTransition.UpdateWithLerpPos(P_LerpParam, ref BuffersData);
        Debug.Log($"Finishing job with {BuffersData.ToString()}");
    }
}