using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Orazum.Meshing;

public abstract class FS_Scaler
{
    protected int2 _index;
    protected float2 _uv;
    protected float2 _originTargetScaleValue;

    protected NativeArray<VertexData> _vertices;
    protected NativeArray<short> _indices;
    protected MeshBuffersIndexersForJob _indexersForJob;

    public void Setup(in int2 index, in float2 originTargetScaleValue)
    { 
        _index = index;
        _originTargetScaleValue = originTargetScaleValue;
    }

    public void AssignMeshBuffers(
        in NativeArray<VertexData> vertices,
        in float2 uv,
        in NativeArray<short> indices,
        in MeshBuffersIndexersForJob indexersForJob)
    {
        _vertices = vertices;
        _uv = uv;
        _indices = indices;
        _indexersForJob = indexersForJob;
    }

    public abstract void AssignGenParams(FigureGenParamsSO genParams);
    public abstract void PrepareJob();
    public abstract JobHandle ScheduleScalingJob(float lerpParam);
}