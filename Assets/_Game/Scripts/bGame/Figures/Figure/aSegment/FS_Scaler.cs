using System;
using Unity.Collections;
using Unity.Mathematics;
using Orazum.Meshing;

public abstract class FS_Scaler
{
    protected float _lerpSpeed;
    protected float _scaleValue;

    protected NativeArray<VertexData> _vertices;
    protected NativeArray<short> _indices;

    public void Initialize(float lerpSpeed, float scaleValue)
    { 
        _lerpSpeed = lerpSpeed;
        _scaleValue = scaleValue;
    }

    public void AssignMeshBuffers(in NativeArray<VertexData> vertices, in NativeArray<short> indices)
    {
        _vertices = vertices;
        _indices = indices;
    }

    public abstract void StartScaling(int2 index, Action completionCallBack);
}