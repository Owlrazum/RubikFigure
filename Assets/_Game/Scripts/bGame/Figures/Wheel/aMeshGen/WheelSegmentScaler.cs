using Unity.Mathematics;
using Unity.Jobs;

using UnityEngine.Assertions;

public class WheelSegmentScaler : FS_Scaler
{
    private WheelGenParamsSO _genParams;
    private WheelScaleJob _job;

    public override void AssignGenParams(FigureGenParamsSO genParams)
    {
        _genParams = genParams as WheelGenParamsSO;
        Assert.IsNotNull(_genParams);
    }

    public override void PrepareJob()
    { 
        _job = new WheelScaleJob()
        {
            P_Index = _index,
            P_UV = _uv,

            P_InnerCircleRadius = _genParams.InnerRadius,
            P_OuterCircleRadius = _genParams.OuterRadius,
            P_SideCount = _genParams.SideCount,
            P_RingCount = _genParams.RingCount,
            P_SegmentResolution = _genParams.SegmentResolution,

            OutVertices = _vertices,
            OutIndices = _indices,
            OutBuffersIndexers = _indexersForJob
        };
    }

    public override JobHandle ScheduleScalingJob(float lerpParam)
    {
        float scaleValue = math.lerp(_originTargetScaleValue.x, _originTargetScaleValue.y, lerpParam);
        _job.P_ScaleValue = scaleValue;
        return _job.Schedule();
    }
}