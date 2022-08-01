using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public struct CircleRays : IEnumerable<float3>
{
    private float3 _r1;
    private float3 _r2;
    private float3 _r3;
    private float3 _r4;
    private float3 _r5;
    private float3 _r6;

    public float3 this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return _r1;
                case 1: return _r2;
                case 2: return _r3;
                case 3: return _r4;
                case 4: return _r5;
                case 5: return _r6;
                default: throw new ArgumentOutOfRangeException($"There are only 12 vertices! You tried to access the vertex at index {index}");
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    _r1 = value;
                    break;
                case 1:
                    _r2 = value;
                    break;
                case 2:
                    _r3 = value;
                    break;
                case 3:
                    _r4 = value;
                    break;
                case 4:
                    _r5 = value;
                    break;
                case 5:
                    _r6 = value;
                    break;
                default: throw new ArgumentOutOfRangeException($"There are only 12 vertices! You tried to access the vertex at index {index}");
            }
        }
    }

    public IEnumerator<float3> GetEnumerator()
    {
        for (int i = 0; i < 6; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}