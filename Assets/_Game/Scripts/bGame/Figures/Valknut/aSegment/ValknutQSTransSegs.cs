using Unity.Collections;

/// <summary>
/// The clockOrder is determined by the origin segment mesh.
/// </summary>
public struct ValknutQSTransSegs
{
    public NativeArray<QSTransSegment>.ReadOnly CW;
    public NativeArray<QSTransSegment>.ReadOnly AntiCW;
}