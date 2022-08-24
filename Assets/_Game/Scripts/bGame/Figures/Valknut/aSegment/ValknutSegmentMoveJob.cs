using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

using Orazum.Meshing;

// [BurstCompile]
public struct ValknutSegmentMoveJob : IJob
{
    public float P_LerpParam;
    public QuadStripTransition InputQuadStripTransition;
    public MeshBuffersData BuffersData;

    public ValknutSegmentMoveJob(ref MeshBuffersData buffersData, ref QuadStripTransition quadStripTransition)
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