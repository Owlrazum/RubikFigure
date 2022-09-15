using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using static Orazum.Constants.Math;
using static Orazum.Meshing.QSTS_BuilderUtils;
using static Orazum.Math.LineSegmentUtilities;
using static QST_Segment;
using static QSTSFD_Radial;
using static QSTS_FillData;

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

        public void Filled(
            in QuadStrip qs,
            float2 lerpRange,
            in float lerpLength,
            bool isNew,
            out QST_Segment qsts
        )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.SingleRotation, lerpLength, out QSTSFD_Radial radial);
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;
            QSTS_FillData fillData = new QSTS_FillData(constructType, FillType.StartToEnd, lerpRange, in radial);
            qsts[0] = fillData;
        }
        #region SingleRotationLerp
        public void FillOut_SRL(
            in QuadStrip qs,
            in float2 lerpRange,
            in float lerpLength,
            bool isNew,
            ClockOrderType clockOrder,
            out QST_Segment qsts
            )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.SingleRotation, lerpLength, out QSTSFD_Radial radial);
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;
            FillType fillType = clockOrder == ClockOrderType.CW ? FillType.ToEnd : FillType.ToStart;
            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        public void FillIn_SRL(
            in QuadStrip qs,
            in float2 lerpRange,
            in float lerpLength,
            bool isNew,
            ClockOrderType clockOrder,
            out QST_Segment qsts
            )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.SingleRotation, lerpLength, out QSTSFD_Radial radial);
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;
            FillType fillType = clockOrder == ClockOrderType.CW ? FillType.FromStart : FillType.FromEnd;
            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        #endregion

        #region DoubleRotationLerp
        public void FillIn_DRL(
            in QuadStrip down,
            in QuadStrip up,
            in float2 lerpRange,
            in float lerpLength,
            bool isNew,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;

            float3x2 start = new float3x2(down[0][0], up[0][1]);
            float3x2 end = new float3x2(down[down.LineSegmentsCount - 1][0], up[up.LineSegmentsCount - 1][1]);
            FillType fillType = vertOrder == VertOrderType.Up ? FillType.ToEnd : FillType.ToStart;

            PrepareSegment(start, end, QSTS_Type.Radial, fillDataLength: 1, out qsts);

            QSTSFD_Radial radial = new QSTSFD_Radial(
                RadialType.DoubleRotation,
                PrimaryAxisAngle,
                SecondaryAngle,
                RotationCenter,
                lerpLength,
                _resolution
            );

            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }
        #endregion

        #region MoveLerp
        public void GenerateSingleMoveLerp(
            in QuadStrip qs,
            in float2 lerpRange,
            in float lerpLength,
            bool isNew,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            float3x2 start = qs[0];
            RadialType radialType = RadialType.SingleMove;
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;
            FillType fillType = vertOrder == VertOrderType.Up ? FillType.ToEnd : FillType.ToStart;

            PrepareSegment(start, float3x2.zero, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(radialType, lerpLength, out QSTSFD_Radial radial);
            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        public void GenerateDoubleMoveLerp(
            in QuadStrip origin,
            in QuadStrip target,
            float2 lerpRange,
            in float lerpLength,
            bool isNew,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            RadialType radialType = RadialType.DoubleMove;
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;
            bool isUp = vertOrder == VertOrderType.Up;
            float3x2 start = isUp ? origin[0] : target[0];
            float3x2 end   = isUp ? target[0] : origin[0];
            FillType fillType = isUp ? FillType.ToEnd : FillType.ToStart;
            
            PrepareSegment(start, end, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(radialType, lerpLength, out QSTSFD_Radial radial);

            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }
        #endregion

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