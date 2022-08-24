using Unity.Mathematics;
using Unity.Collections;

/// <summary>
/// The clockOrder is determined by the origin segment mesh.
/// </summary>
public struct ValknutTransitionData
{
    public NativeArray<float4x2>.ReadOnly PositionsCW;
    public NativeArray<float3>.ReadOnly LerpRangesCW;

    public NativeArray<float4x2>.ReadOnly PositionsCCW;
    public NativeArray<float3>.ReadOnly LerpRangesCCW;
}