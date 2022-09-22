using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using static Orazum.Constants.Math;
using static Orazum.Meshing.QSTS_BuilderUtils;
using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Meshing.QST_Segment;
using static Orazum.Meshing.QSTSFD_Radial;
using static Orazum.Meshing.QSTS_FillData;

namespace Orazum.Meshing
{
    public struct QSTS_RadialBuilder
    {
        // SRL: SingleRotationLerp
        // DRL: DoubleRotationLerp
        public float3 RotationCenter { get; set; }

        private readonly float4 PrimaryAxisAngle;
        private readonly float SecondaryAngle;
        private readonly int _resolution;

        public QSTS_RadialBuilder(float3 primaryAxis, float2 anglesRad, int resolution)
        {
            PrimaryAxisAngle = new float4(primaryAxis, anglesRad.x);
            SecondaryAngle = anglesRad.y;
            RotationCenter = float3.zero;
            _resolution = resolution;
        }

        public struct Parameters
        {
            public float2 LerpRange;
            public float LerpLength;
            public ConstructType Construct;
            public bool IsTemporary;
            public FillType Fill;
        }

        public static Parameters DefaultParameters()
        {
            return new Parameters()
            {
                LerpRange = new float2(0, 1),
                LerpLength = 1,
                Construct = ConstructType.New,
                IsTemporary = false,
                Fill = FillType.StartToEnd
            };
        }

        public void Filled(
            in QuadStrip qs,
            float2 lerpRange,
            in float lerpLength,
            ConstructType constructType,
            out QST_Segment qsts
        )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.FirstOrderRotation, lerpLength, out QSTSFD_Radial radial);
            QSTS_FillData fillData = new QSTS_FillData(constructType, FillType.StartToEnd, lerpRange, in radial);
            qsts[0] = fillData;
        }

        public void SingleRotationLerp(
            in QuadStrip qs,
            in Parameters p,
            out QST_Segment qsts
            )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.FirstOrderRotation, p.LerpLength, out QSTSFD_Radial radial);
            QSTS_FillData fillData = new QSTS_FillData(p.Construct, p.Fill, p.LerpRange, in radial);
            fillData.IsTemporary = p.IsTemporary;
            qsts[0] = fillData;
        }

        public void MoveLerp(
            in QuadStrip qs,
            in Parameters p,
            out QST_Segment qsts
        )
        {
            RadialType radialType = RadialType.Move;
            PrepareSegment(qs[0], float3x2.zero, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(radialType, p.LerpLength, out QSTSFD_Radial radial);

            QSTS_FillData fillData = new QSTS_FillData(p.Construct, p.Fill, p.LerpRange, in radial);
            fillData.IsTemporary = p.IsTemporary;
            qsts[0] = fillData;
        }

        private void PrepareRadial(RadialType radialType, in float lerpLength, out QSTSFD_Radial radial)
        {
            radial = new QSTSFD_Radial(
                radialType,
                PrimaryAxisAngle,
                secondaryAngle: 0,
                RotationCenter,
                lerpLength,
                _resolution
            );
        }
    }
}

/*
        public void DoubleRotationLerp(
            in QuadStrip down,
            in QuadStrip up,
            in float2 lerpRange,
            in float lerpLength,
            bool isNew,
            bool isTemporary,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;

            float3x2 start = new float3x2(down[0][0], up[0][1]);
            float3x2 end = new float3x2(down[down.LineSegmentsCount - 1][0], up[up.LineSegmentsCount - 1][1]);
            FillType fillType = vertOrder == VertOrderType.Up ? FillType.FromStart : FillType.FromEnd;

            PrepareSegment(start, end, QSTS_Type.Radial, fillDataLength: 1, out qsts);

            QSTSFD_Radial radial = new QSTSFD_Radial(
                RadialType.SecondOrderRotation,
                PrimaryAxisAngle,
                SecondaryAngle,
                RotationCenter,
                lerpLength,
                _resolution
            );

            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            fillData.IsTemporary = isTemporary;
            qsts[0] = fillData;
        }
*/