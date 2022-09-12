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
            PrepareRadial(RadialType.SingleRotationLerp, out QSTSFD_Radial radial);
            FillType fillType = isNew ? FillType.NewStartToEnd : FillType.ContinueStartToEnd;
            QSTS_FillData fillData = new QSTS_FillData(fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }
        #region SingleRotationLerp
        public void FillOut_SRL(
            in QuadStrip qs,
            in float2 lerpRange,
            ClockOrderType clockOrder,
            out QST_Segment qsts
            )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.SingleRotationLerp, out QSTSFD_Radial radial);
            FillType fillType = clockOrder == ClockOrderType.CW ? FillType.NewToEnd : FillType.NewToStart;
            QSTS_FillData fillData = new QSTS_FillData(fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        public void FillIn_SRL(
            in QuadStrip qs,
            in float2 lerpRange,
            ClockOrderType clockOrder,
            out QST_Segment qsts
            )
        {
            PrepareSegment(qs, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(RadialType.SingleRotationLerp, out QSTSFD_Radial radial);
            FillType fillType = clockOrder == ClockOrderType.CW ? FillType.NewFromStart : FillType.NewFromEnd;
            QSTS_FillData fillData = new QSTS_FillData(fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        #endregion

        #region DoubleRotationLerp
        public void GenerateLevitaion(
            in QuadStrip origin,
            in QuadStrip target,
            float3 lerpRangeLerpLength,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            float3x2 start, end;
            RadialType radialType;
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

                    radialType = RadialType.DoubleRotationLerpDown;
                    fillType = FillType.NewToEnd;
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

                    radialType = RadialType.DoubleRotationLerpUp;
                    fillType = FillType.NewToStart;
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

            QSTS_FillData fillData = new QSTS_FillData(fillType, lerpRangeLerpLength.xy, in radial);
            qsts[0] = fillData;
        }
        #endregion

        #region MoveLerp
        public void GenerateSingleMoveLerp(
            in QuadStrip qs,
            float2 lerpRange,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            float3x2 start = qs[0];
            RadialType radialType;
            FillType fillType;
            switch (vertOrder)
            {
                case VertOrderType.Down:
                    radialType = RadialType.SingleMoveLerpDown;
                    fillType = FillType.NewToStart;
                    break;
                case VertOrderType.Up:
                    radialType = RadialType.SingleMoveLerpUp;
                    fillType = FillType.NewToEnd;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown VertOrderType");
            }

            PrepareSegment(start, float3x2.zero, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(radialType, out QSTSFD_Radial radial);
            QSTS_FillData fillData = new QSTS_FillData(fillType, lerpRange, in radial);
            qsts[0] = fillData;
        }

        public void GenerateDoubleMoveLerp(
            in QuadStrip origin,
            in QuadStrip target,
            float2 lerpRange,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            float3x2 start, end;
            RadialType radialType;
            FillType fillType;
            switch (vertOrder)
            {
                case VertOrderType.Down:
                    start = target[0];
                    end = origin[0];

                    radialType = RadialType.DoubleMoveLerpDown;
                    fillType = FillType.NewToStart;
                    break;
                case VertOrderType.Up:
                    start = origin[0];
                    end = target[0];

                    radialType = RadialType.DoubleMoveLerpDown;
                    fillType = FillType.NewToStart;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown VertOrderType");
            }
            PrepareSegment(start, end, QSTS_Type.Radial, fillDataLength: 1, out qsts);
            PrepareRadial(radialType, out QSTSFD_Radial radial);

            QSTS_FillData fillData = new QSTS_FillData(fillType, lerpRange, in radial);
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