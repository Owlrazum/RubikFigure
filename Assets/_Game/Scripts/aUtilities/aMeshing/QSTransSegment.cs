using Unity.Mathematics;
using Unity.Collections;

public struct QSTransSegment
{
    public float2x2 StartLineSegment { get; private set; }
    public float2x2 EndLineSegment { get; private set; }
    public int FillDataLength { get; private set; }
    
    private QSTransSegFillData _f1;
    private QSTransSegFillData _f2;
    private QSTransSegFillData _f3;

    public QSTransSegment(float2x2 startLineSegment, float2x2 endLineSegment, int fillDataLength)
    { 
        StartLineSegment = startLineSegment;
        EndLineSegment = endLineSegment;
        FillDataLength = fillDataLength;

        _f1 = new QSTransSegFillData();
        _f2 = new QSTransSegFillData();
        _f3 = new QSTransSegFillData();
    }

    public QSTransSegFillData this[int index]
    {
        get
        {
            if (index >= FillDataLength)
            { 
                throw new System.ArgumentOutOfRangeException("QSTransSegment supports max 3 QSTransSegFillData");
            }

            switch (index)
            { 
                case 0:
                    return _f1;
                case 1:
                    return _f2;
                case 2:
                    return _f3;
            }

            throw new System.Exception();
        }

        set
        {
            if (index >= FillDataLength)
            { 
                throw new System.ArgumentOutOfRangeException("QSTransSegment supports max 3 QSTransSegFillData");
            }

            switch (index)
            { 
                case 0:
                    _f1 = value;
                    return;
                case 1:
                    _f2 = value;
                    return;
                case 2:
                    _f3 = value;
                    return;
            }
        }
    }

    public enum QuadConstructType 
    {
        NewQuadStartToEnd,
        ContinueQuadStartToEnd,
        ContinueQuadFromStart,
        NewQuadToEnd
    }
}