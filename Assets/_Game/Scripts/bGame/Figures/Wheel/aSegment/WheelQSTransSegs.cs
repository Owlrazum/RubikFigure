using Unity.Collections;

public struct WheelQSTransSegs
{ 
    public NativeArray<QSTransSegment>.ReadOnly Atsi;
    public NativeArray<QSTransSegment>.ReadOnly Ctsi;
    public NativeArray<QSTransSegment>.ReadOnly Dtsi;
    public NativeArray<QSTransSegment>.ReadOnly Utsi;
}