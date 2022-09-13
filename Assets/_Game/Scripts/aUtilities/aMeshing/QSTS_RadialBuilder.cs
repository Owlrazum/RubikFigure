using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
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
        public float3x2 Points { get; set; } // the first one is the center of rotation

        private float4x2 ConstructAxisAngles;
        private int _resolution;

        public QSTS_RadialBuilder(float3 rotationAxis, float angleDeltaRad, int resolution)
        {
            ConstructAxisAngles = new float4x2(new float4(rotationAxis, angleDeltaRad * resolution), float4.zero);
            Points = float3x2.zero;
            _resolution = resolution;
        }

        public void Filled(
            in QuadStrip qs,
            float2 lerpRange,
            bool isNew,
            out QST_Segment qsts
        )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.SingleRotation, out QSTSFD_Radial radial);
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;
            QSTS_FillData fillData = new QSTS_FillData(constructType, FillType.StartToEnd, lerpRange, in radial);
            qsts[0] = fillData;
        }
        #region SingleRotationLerp
        public void FillOut_SRL(
            in QuadStrip qs,
            in float2 lerpRange,
            bool isNew,
            ClockOrderType clockOrder,
            out QST_Segment qsts
            )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.SingleRotation, out QSTSFD_Radial radial);
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;
            FillType fillType = clockOrder == ClockOrderType.CW ? FillType.ToEnd : FillType.ToStart;
            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        public void FillIn_SRL(
            in QuadStrip qs,
            in float2 lerpRange,
            bool isNew,
            ClockOrderType clockOrder,
            out QST_Segment qsts
            )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.SingleRotation, out QSTSFD_Radial radial);
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;
            FillType fillType = clockOrder == ClockOrderType.CW ? FillType.FromStart : FillType.FromEnd;
            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        #endregion

        #region DoubleRotationLerp
        public void GenerateLevitaion(
            in QuadStrip origin,
            in QuadStrip target,
            float3 lerpRangeLerpLength,
            bool isNew,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            RadialType radialType = RadialType.DoubleRotation;
            ConstructType constructType = isNew ? ConstructType.New : ConstructType.Continue;

            float3x2 start, end;
            FillType fillType;
            switch (vertOrder)
            {
                case VertOrderType.Down:
                    start = new float3x2(
                        origin[0][0],
                        origin[origin.LineSegmentsCount - 1][0]
                    );

                    end = new float3x2(
                        target[0][1],
                        target[target.LineSegmentsCount - 1][1]
                    );


                    //!
                    fillType = FillType.ToStart;
                    break;
                case VertOrderType.Up:
                    start = new float3x2(
                        target[0][0],
                        target[origin.LineSegmentsCount - 1][0]
                    );

                    end = new float3x2(
                        origin[0][1],
                        origin[target.LineSegmentsCount - 1][1]
                    );


                    //!
                    fillType = FillType.ToEnd;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown VertOrderType");
            }

            PrepareSegment(start, end, QSTS_Type.Radial, 1, out qsts);

            quaternion perp = quaternion.AxisAngle(math.up(), -90);
            float3x2 startLS = origin[0];
            float3x2 endLS = origin[origin.LineSegmentsCount - 1];
            float4x2 axisAngles = new float4x2(
                new float4(GetDirection(perp, startLS), 180),
                new float4(GetDirection(perp, endLS), 180)
            );

            float3x2 points = new float3x2(
                GetLineSegmentCenter(start[0], end[0]),
                GetLineSegmentCenter(start[1], end[1])
            );

            QSTSFD_Radial radial = new QSTSFD_Radial(
                radialType,
                in axisAngles,
                in points,
                lerpLength: lerpRangeLerpLength.z,
                _resolution
            );

            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRangeLerpLength.xy, in radial);
            qsts[0] = fillData;
        }
        #endregion

        #region MoveLerp
        public void GenerateSingleMoveLerp(
            in QuadStrip qs,
            float2 lerpRange,
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
            PrepareRadial(radialType, out QSTSFD_Radial radial);
            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        public void GenerateDoubleMoveLerp(
            in QuadStrip origin,
            in QuadStrip target,
            float2 lerpRange,
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
            PrepareRadial(radialType, out QSTSFD_Radial radial);

            QSTS_FillData fillData = new QSTS_FillData(constructType, fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }
        #endregion

        private void PrepareRadial(RadialType radialType, out QSTSFD_Radial radial)
        {
            radial = new QSTSFD_Radial(
                radialType,
                ConstructAxisAngles,
                Points,
                lerpLength: 1,
                _resolution
            );
        }
    }
}