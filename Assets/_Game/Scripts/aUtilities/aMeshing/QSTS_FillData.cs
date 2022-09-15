using System;
using Unity.Mathematics;
using UnityEngine.Assertions;

public struct QSTS_FillData
{
    public enum ConstructType
    { 
        New, // QuadStripStart
        Continue // QuadStripContinue
    }
    public ConstructType Construct { get; private set; }

    public enum FillType
    {
        StartToEnd,
        FromStart,
        FromEnd,
        ToStart,
        ToEnd
    }
    public FillType Fill { get; private set; }

    public float2 LerpRange { get; set; }

    public QSTSFD_Radial Radial { get; set; }

    public QSTS_FillData(ConstructType construct, FillType fill, float2 lerpRange)
    {
        Construct = construct;
        Fill = fill;
        LerpRange = lerpRange;
        Radial = new QSTSFD_Radial(-1);
    }

    public QSTS_FillData(ConstructType construct, FillType fill, float2 lerpRange, in QSTSFD_Radial radial)
    {
        Construct = construct;
        Fill = fill;
        LerpRange = lerpRange;
        Radial = radial;
        Assert.IsTrue(radial.MaxLerpLength > 0);
    }

    public override string ToString()
    {
        return $"{Fill} {LerpRange.x:F2} {LerpRange.y:F2}" + (Radial.MaxLerpLength > 0 ? $"{Radial}" : "");
    }

    public struct FillTypeLerpConstruct
    {
        public readonly int AddStart;
        public readonly int AddEnd;
        public readonly int AddLerpParam;
        public readonly bool AddLerpAtStart;
        public readonly bool AddLerpAtEnd;
      
        public readonly float2 LerpOffset;
        private readonly float LerpLength;

        public FillTypeLerpConstruct(FillType fillType, in float maxLerpLength, ref float lerpParam)
        { 
            LerpOffset = float2.zero;

            AddStart = 0; AddEnd = 0; AddLerpParam = 0; 
            AddLerpAtEnd = false; AddLerpAtStart = false; 

            if (fillType == FillType.FromEnd || fillType == FillType.ToStart)
            {
                lerpParam = 1 - lerpParam;
            }
            switch (fillType)
            {
                case FillType.FromStart:
                    AddLerpAtEnd = true;
                    AddLerpParam = 1;

                    LerpOffset.y = lerpParam;

                    float lengthStart = lerpParam - maxLerpLength;
                    if (lengthStart > 0)
                    {
                        LerpOffset.x = lengthStart;
                    }
                    else
                    {
                        AddStart = 1;
                        LerpOffset.x = 0;
                    }
                    break;
                case FillType.ToEnd:
                    AddLerpAtStart = true;
                    AddLerpParam = 1;

                    LerpOffset.x = lerpParam;

                    float lengthEnd = lerpParam + maxLerpLength;
                    if (lengthEnd < 1)
                    {
                        LerpOffset.y = lengthEnd;
                    }
                    else
                    {
                        AddEnd = 1;
                        LerpOffset.y = 1;
                    }
                    break;
                case FillType.FromEnd:
                    AddLerpAtStart = true;
                    AddLerpParam = 1;

                    LerpOffset.x = lerpParam;

                    lengthEnd = lerpParam + maxLerpLength;
                    if (lengthEnd < 1)
                    {
                        LerpOffset.y = lengthEnd;
                    }
                    else
                    {
                        AddEnd = 1;
                        LerpOffset.y = 1;
                    }
                    break;
                case FillType.ToStart:
                    AddLerpAtEnd = true;
                    AddLerpParam = 1;

                    LerpOffset.y = lerpParam;

                    lengthStart = lerpParam - maxLerpLength;
                    if (lengthStart > 0)
                    {
                        LerpOffset.x = lengthStart;
                    }
                    else
                    {
                        AddStart = 1;
                        LerpOffset.x = 0;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"This fillType {fillType} is not supported for RotationLerp");
            }

            LerpLength = LerpOffset.y - LerpOffset.x;
        }

        public int GetSegsCount(in float lerpDelta, out int deltaSegsCount)
        {
            deltaSegsCount = GetDeltaSegsCount(lerpDelta);
            return deltaSegsCount + AddStart + AddEnd + AddLerpParam;
        }

        public int GetDeltaSegsCount(in float lerpDelta)
        {
            return (int)(LerpLength / lerpDelta);
        }
    }
}