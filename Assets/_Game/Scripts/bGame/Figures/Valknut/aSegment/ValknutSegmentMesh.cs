using UnityEngine.Assertions;
using Unity.Mathematics;
using static Orazum.Math.MathUtilities;

public struct ValknutSegmentMesh
{
    private bool _isHavingCenterQuad;
    private float2x4 _leftQuad;
    private float2x4 _centerQuad;
    private float2x4 _rightQuad;

    public ValknutSegmentMesh(in float2x4 leftQuad, in float2x4 centerQuad, in float2x4 rightQuad)
    {
        _isHavingCenterQuad = true;
        
        _leftQuad = leftQuad;
        _centerQuad = centerQuad;
        _rightQuad = rightQuad;
    }

    public ValknutSegmentMesh(in float2x4 leftQuad, in float2x4 rightQuad)
    { 
        _isHavingCenterQuad = false;
        
        _leftQuad = leftQuad;
        _centerQuad = float2x4.zero;
        _rightQuad = rightQuad;
    }

    public bool DoesHaveCenterQuad()
    {
        return _isHavingCenterQuad;
    }

    public float3 GetPointVertexPos(int pointVertexIndex)
    {
        int index = pointVertexIndex % 4;
        switch (pointVertexIndex / 4)
        {
            case 0:
                return GetQuadVertexPos(in _leftQuad, index);
            case 1:
                if (_isHavingCenterQuad)
                {
                    return GetQuadVertexPos(in _centerQuad, index);
                }
                else
                {
                    return GetQuadVertexPos(in _rightQuad, index);
                }
            case 2:
                Assert.IsFalse(_isHavingCenterQuad);
                return GetQuadVertexPos(in _rightQuad, index);
        }
        throw new System.ArgumentOutOfRangeException($"pointVertexIndex {pointVertexIndex} should be in [0..11]");
    }

    private float3 GetQuadVertexPos(in float2x4 quad, int index)
    {
        switch (index)
        { 
            case 0:
                return x0z(quad[0]);
            case 1:
                return x0z(quad[1]);
            case 2:
                return x0z(quad[2]);
            case 3:
                return x0z(quad[3]);
        }

        throw new System.ArgumentOutOfRangeException($"index {index} should be in [0..3]");
    }

    public int2 GetSegmentIndices(int pointVertexIndex)
    {
        return int2.zero;
    }
}