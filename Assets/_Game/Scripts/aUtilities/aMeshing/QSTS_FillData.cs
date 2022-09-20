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

    public bool IsTemporary { get; set; }
    // SegmentType is assigned when segment receives fillData.
    public QST_Segment.QSTS_Type SegmentType { get; set; }

    public float2 LerpRange { get; set; }

    public QSTSFD_Radial Radial { get; set; }

    public QSTS_FillData(ConstructType construct, FillType fill, float2 lerpRange)
    {
        Construct = construct;
        Fill = fill;
        LerpRange = lerpRange;
        Radial = new QSTSFD_Radial(-1);
        IsTemporary = false;
        SegmentType = QST_Segment.QSTS_Type.Quad;
    }

    public QSTS_FillData(ConstructType construct, FillType fill, float2 lerpRange, in QSTSFD_Radial radial)
    {
        Assert.IsTrue(radial.MaxLerpLength > 0);
        Construct = construct;
        Fill = fill;
        LerpRange = lerpRange;
        Radial = radial;
        IsTemporary = true;
        SegmentType = QST_Segment.QSTS_Type.Quad;
    }

    public override string ToString()
    {
        return $"{Fill} {LerpRange.x:F2} {LerpRange.y:F2}" + (Radial.MaxLerpLength > 0 ? $"{Radial}" : "");
    }

    public struct FillTypeLerpConstruct // for complex cases
    {
        public readonly int AddStart;
        public readonly int AddEnd;
        public readonly bool AddLerpAtStart;
        public readonly bool AddLerpAtEnd;

        public readonly float2 LerpOffset;
        public readonly float LerpLength;
        public readonly float LerpDelta;

        public readonly int DeltaSegsCount;
        public readonly int SegsCount;

        private readonly int _addLerpParam;

        public FillTypeLerpConstruct(in QSTS_FillData fillData, in float maxLerpLength, ref float lerpParam)
        {
            Assert.IsTrue(fillData.Fill == FillType.FromStart || fillData.Fill == FillType.ToEnd || fillData.Fill == FillType.FromEnd || fillData.Fill == FillType.ToStart, "All except FillType.FromStartToEnd, 4 exactly, are supported");
            FillType fillType = fillData.Fill;
            LerpOffset = float2.zero;

            _addLerpParam = 1;

            AddStart = 0; AddEnd = 0;
            AddLerpAtEnd = false; AddLerpAtStart = false;

            if (fillType == FillType.FromEnd || fillType == FillType.ToStart)
            {
                lerpParam = 1 - lerpParam;
            }

            if (fillData.IsTemporary)
            {
                lerpParam *= 1 + 1 / maxLerpLength;
            }

            if (fillType == FillType.FromStart || fillType == FillType.ToStart)
            {
                AddLerpAtEnd = true;
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
            }
            else if (fillType == FillType.FromEnd || fillType == FillType.ToEnd)
            {
                AddLerpAtStart = true;
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
            }

            // if (fillType == FillType.FromStart || fillType == FillType.FromEnd)
            // {
            //     LerpOffset.y += maxLerpLength;
            // }

            int resolution;
            if (fillData.SegmentType == QST_Segment.QSTS_Type.Quad)
            {
                resolution = 1;
            }
            else if (fillData.SegmentType == QST_Segment.QSTS_Type.Radial)
            {
                resolution = fillData.Radial.Resolution;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"The {fillData.SegmentType} is unknown");
            }

            LerpLength = LerpOffset.y - LerpOffset.x;
            if (LerpLength == 0)
            {
                LerpLength = 0.1f;
            }
            LerpDelta = LerpLength / resolution;
            DeltaSegsCount = (int)(LerpLength / LerpDelta);
            SegsCount = DeltaSegsCount + AddStart + AddEnd + _addLerpParam;
        }

        public override string ToString()
        {
            return $"{LerpOffset} {LerpLength} {AddStart}{AddEnd}{(AddLerpAtStart ? 1 : 0)}{(AddLerpAtEnd ? 1 : 0)}";
        }
    }
}