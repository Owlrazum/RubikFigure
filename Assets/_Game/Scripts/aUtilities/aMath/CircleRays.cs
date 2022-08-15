using Unity.Mathematics;
using static Orazum.Math.MathUtilities;

/// <summary>
/// Computes rays starting at 12 hour and moves clockwise.
/// </summary>
public struct CircleRays
{
    private float3 _r0;
    private float3 _r1;
    private float3 _r2;
    private float3 _r3;
    private float3 _r4;
    private float3 _r5;

    private int _raysCount;
    private float _angleDelta;
    private float _startAngle;

    public void ComputeRays(int raysCountArg)
    {
        if (raysCountArg < 2 || raysCountArg > 6)
        {
            throw new System.ArgumentException($"Not supported raysCount {raysCountArg}!");
        }
        _raysCount = raysCountArg;

        _angleDelta = TAU / raysCountArg;
        _startAngle = TAU / 4;
        float currentAngle = _startAngle;

        // we subtract so the positive would be clockwiseOrder,
        // with addition it will be counter-clockwise;
        _r0 = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
        currentAngle -= _angleDelta;
        _r1 = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
        currentAngle -= _angleDelta;
        if (_raysCount >= 3)
        { 
            _r2 = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle -= _angleDelta;
        }
        if (_raysCount >= 4)
        { 
            _r3 = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle -= _angleDelta;
        }
        if (_raysCount >= 5)
        { 
            _r4 = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle -= _angleDelta;
        }
        if (_raysCount >= 6)
        { 
            _r5 = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle -= _angleDelta;
        }
    }

    public float3 GetRayBetween(int leftIndex, float lerpParam)
    {
        float leftAngle = _angleDelta * leftIndex;
        float rightAngle = leftAngle + _angleDelta;
        float betweenAngle = math.lerp(leftAngle, rightAngle, lerpParam);
        quaternion q = quaternion.AxisAngle(math.up(), betweenAngle);

        return  math.rotate(q, _r0);
    }

    public float GetRayAngle(int rayIndex)
    {
        return _angleDelta * rayIndex;
    }

    public float3 this[int index]
    {
        get
        {
            if (index >= _raysCount)
            { 
                throw new System.ArgumentOutOfRangeException($"index {index} is out of raysCount {_raysCount}");
            }
            switch (index)
            {
                case 0: return _r0;
                case 1: return _r1;
                case 2: return _r2;
                case 3: return _r3;
                case 4: return _r4;
                case 5: return _r5;

                default: throw new System.ArgumentException();
            }
        }
        set
        {
            if (index >= _raysCount)
            { 
                throw new System.ArgumentOutOfRangeException($"index {index} is out of raysCount {_raysCount}");
            }
            switch (index)
            {
                case 0:
                    _r0 = value;
                    break;
                case 1:
                    _r1 = value;
                    break;
                case 2:
                    _r2 = value;
                    break;
                case 3:
                    _r3 = value;
                    break;
                case 4:
                    _r4 = value;
                    break;
                case 5:
                    _r5 = value;
                    break;
                default: throw new System.ArgumentException();
            }
        }
    }
}