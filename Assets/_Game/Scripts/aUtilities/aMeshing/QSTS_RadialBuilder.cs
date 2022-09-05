using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using static Orazum.Math.LineSegmentUtilities;
using static QST_Segment;
using static QSTSFD_Radial;
using static QSTS_FillData;

namespace Orazum.Meshing
{
    public struct QSTS_RadialBuilder
    {
        // SRL: SingleRotationLerp
        public float4 SRL_AxisAngleCW { get; set; }
        public float4 SRL_AxisAngleAntiCW { get; set; }

        public ClockOrderType ClockOrder { get; set; }
        public int Resolution { get; set; }

        #region SingleRotationLerp
        public void Filled(
            in QuadStrip qs,
            FillType fillType,
            out QST_Segment qsts
        )
        {
            Assert.IsTrue(fillType == FillType.NewStartToEnd || fillType == FillType.ContinueStartToEnd);
            GenerateSingleRotationLerp(in qs, fillType, out qsts);
        }

        public void FillOut(
            in QuadStrip qs,
            FillType fillType,
            out QST_Segment qsts
        )
        {
            Assert.IsTrue(fillType == FillType.NewToEnd || fillType == FillType.ContinueToEnd);
            GenerateSingleRotationLerp(in qs, fillType, out qsts);
        }

        public void FillIn(
            in QuadStrip qs,
            FillType fillType,
            out QST_Segment qsts
        )
        {
            Assert.IsTrue(fillType == FillType.NewFromStart || fillType == FillType.ContinueFromStart);
            GenerateSingleRotationLerp(in qs, fillType, out qsts);
        }

        private void GenerateSingleRotationLerp(
            in QuadStrip qs,
            FillType fillType,
            out QST_Segment qsts)
        {
            float3x2 startLineSeg = float3x2.zero;
            switch (ClockOrder)
            {
                case ClockOrderType.CW:
                    startLineSeg = qs[0];
                    break;
                case ClockOrderType.AntiCW:
                    startLineSeg = qs[qs.LineSegmentsCount - 1];
                    break;
            }

            qsts = new QST_Segment(startLineSeg, float3x2.zero, 1);
            qsts.Type = QSTS_Type.Radial;
            float4x2 axisAngles = new float4x2(
                ClockOrder == ClockOrderType.CW ? SRL_AxisAngleCW : SRL_AxisAngleAntiCW,
                float4.zero
            );

            QSTSFD_Radial radial = new QSTSFD_Radial(
                RadialType.SingleRotationLerp,
                in axisAngles,
                in float3x2.zero,
                lerpLength: 1,
                Resolution
            );
            QSTS_FillData fillData = new QSTS_FillData(fillType, new float2(0, 1), in radial);
            qsts[0] = fillData;
        }
        #endregion

        #region DoubleRotationLerp
        public void GenerateDoubleRotationLerp(
            in QuadStrip origin,
            in QuadStrip target,
            VertOrderType vertOrder,
            out QST_Segment qsts
        )
        {
            quaternion perp;
            float3x2 startLineSeg = float3x2.zero;
            float3x2 endLineSeg = float3x2.zero;
            if (vertOrder == VertOrderType.Up)
            {
                perp = quaternion.AxisAngle(math.up(), -90);
                startLineSeg = new float3x2(
                    origin[0][1],
                    origin[origin.LineSegmentsCount - 1][1]
                );
                endLineSeg = new float3x2(
                    target[0][0],
                    target[target.LineSegmentsCount - 1][0]
                );
            }
            else
            {
                startLineSeg = new float3x2(
                    origin[0][0],
                    origin[origin.LineSegmentsCount - 1][0]
                );
                endLineSeg = new float3x2(
                    target[0][1],
                    target[target.LineSegmentsCount - 1][1]
                );
                perp = quaternion.AxisAngle(math.up(), 90);
            }

            qsts = new QST_Segment(startLineSeg, endLineSeg, 1);
            qsts.Type = QSTS_Type.Radial;

            float3x2 startLS = origin[0];
            float3x2 endLS = origin[origin.LineSegmentsCount - 1];

            float4x2 axisAngles = new float4x2(
                new float4(GetPerpDirection(perp, startLS), 180),
                new float4(GetPerpDirection(perp, endLS), 180)
            );

            float3x2 points = new float3x2(
                GetLineSegmentCenter(startLineSeg[0], endLineSeg[0]),
                GetLineSegmentCenter(startLineSeg[1], endLineSeg[1])
            );

            QSTSFD_Radial radial = new QSTSFD_Radial(
                RadialType.MoveLerp,
                in axisAngles,
                in points,
                1,
                Resolution
            );
            QSTS_FillData fillData = new QSTS_FillData(FillType.NewStartToEnd, new float2(0, 1), in radial);
            qsts[0] = fillData;
        }
        #endregion

        #region MoveLerp
        public void GenerateMoveLerp(
            in QuadStrip origin,
            in QuadStrip target,
            out QST_Segment qsts
        )
        {
            float3x2 startLineSeg = new float3x2(
                origin[0][0],
                origin[origin.LineSegmentsCount - 1][0]
            );
            float3x2 points = new float3x2(
                float3.zero,
                origin[0][1]
            );

            qsts = new QST_Segment(startLineSeg, float3x2.zero, 1);
            qsts.Type = QSTS_Type.Radial;

            float4x2 axisAngle = new float4x2(
                SRL_AxisAngleCW,
                float4.zero
            );

            QSTSFD_Radial radial = new QSTSFD_Radial(
                RadialType.MoveLerp,
                in axisAngle,
                in points,
                1,
                Resolution
            );

            QSTS_FillData fillData = new QSTS_FillData(FillType.NewStartToEnd, new float2(0, 1), in radial);
            qsts[0] = fillData;
        }
        #endregion
    }
}