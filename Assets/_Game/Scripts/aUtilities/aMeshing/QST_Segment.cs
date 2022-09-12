using Unity.Mathematics;

public struct QST_Segment
{
    public enum QSTS_Type
    {
        Quad,
        Radial
    }
    public QSTS_Type Type { get; set; }

    public float3x2 StartLineSegment { get; private set; }
    public float3x2 EndLineSegment { get; private set; }
    public int FillDataLength { get; private set; }
    
    private QSTS_FillData _f1;
    private QSTS_FillData _f2;
    private QSTS_FillData _f3;

    public QST_Segment(in float3x2 startLineSegment, in float3x2 endLineSegment, int fillDataLength)
    {
        Type = QSTS_Type.Quad;

        StartLineSegment = startLineSegment;
        EndLineSegment = endLineSegment;
        FillDataLength = fillDataLength;

        _f1 = new QSTS_FillData();
        _f2 = new QSTS_FillData();
        _f3 = new QSTS_FillData();
    }

    public QSTS_FillData this[int index]
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

    public override string ToString()
    {
        string s = "QSTransSegment:\n";
        for (int i = 0; i < FillDataLength; i++)
        { 
            switch (i)
            { 
                case 0:
                    s += _f1.ToString() + "\n";
                    break;
                case 1:
                    s += _f2.ToString() + "\n";
                    break;
                case 2:
                    s += _f3.ToString() + "\n";
                    break;
            }
        }
        return s;
    }

    public string FillTypes()
    {
        string s = "fillTypes:\n";
        for (int i = 0; i < FillDataLength; i++)
        { 
            switch (i)
            { 
                case 0:
                    s += _f1.Fill + "\n";
                    break;
                case 1:
                    s += _f2.Fill + "\n";
                    break;
                case 2:
                    s += _f3.Fill + "\n";
                    break;
            }
        }
        return s;
    }
}