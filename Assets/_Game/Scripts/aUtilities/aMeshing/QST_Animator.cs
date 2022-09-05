using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Math.MathUtils;
using static QST_Segment;
using static QSTS_FillData;
using static QSTSFD_Radial;

namespace Orazum.Meshing
{
    public struct QST_Animator
    {
        private QS_Transition _transition;
        private QuadStripBuilder _quadStripBuilder;
        private float _globalLerpParam;

        public QST_Animator(
            ref NativeArray<VertexData> vertices,
            ref NativeArray<short> indices,
            in float3x2 normalUV
        )
        {
            _transition = new QS_Transition();
            _quadStripBuilder = new QuadStripBuilder(vertices, indices, normalUV);
            _globalLerpParam = -1;
        }

        public void AssignTransition(QS_Transition transition)
        {
            _transition = transition;
        }

        // The order of transSegments is should not be assumed
        public void UpdateWithLerpPos(float globalLerpParam, bool shouldReorientVertices, ref MeshBuffersIndexers buffersIndexers)
        {
            _globalLerpParam = globalLerpParam;

            for (int i = 0; i < _transition.Length; i++)
            {
                QST_Segment segment = _transition[i];
                for (int j = 0; j < segment.FillDataLength; j++)
                {
                    ConsiderFillData(in segment, segment[j], ref buffersIndexers);
                }
            }

            if (shouldReorientVertices)
            {
                _quadStripBuilder.ReorientVertices(buffersIndexers.Count.x);
            }
        }

        private void ConsiderFillData(
            in QST_Segment segment,
            in QSTS_FillData fillData,
            ref MeshBuffersIndexers buffersIndexers)
        {
            if (_globalLerpParam >= fillData.LerpRange.x && _globalLerpParam <= fillData.LerpRange.y)
            {
                float3x4 startEndLineSegs = new float3x4(
                    segment.StartLineSegment[0],
                    segment.StartLineSegment[1],
                    segment.EndLineSegment[0],
                    segment.EndLineSegment[1]
                );

                if (segment.Type == QSTS_Type.Quad)
                {
                    ConstructWithQuadType(
                        in fillData,
                        in startEndLineSegs,
                        ref buffersIndexers
                    );
                }
                else if (segment.Type == QSTS_Type.Radial)
                {
                    ConstructWithRadialType(
                        in fillData,
                        in startEndLineSegs,
                        ref buffersIndexers
                    );
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Unknown MeshConstructType");
                }
            }
        }

        private void ConstructWithQuadType(
            in QSTS_FillData fillData,
            in float3x4 startEndLineSegs,
            ref MeshBuffersIndexers buffersIndexers)
        {
            FillType fillType = fillData.Fill;
            float2 lerpRange = fillData.LerpRange;
            float3x2 start = new float3x2(startEndLineSegs[0], startEndLineSegs[1]);
            float3x2 end = new float3x2(startEndLineSegs[2], startEndLineSegs[3]);

            if (fillType == FillType.NewStartToEnd ||
                fillType == FillType.ContinueStartToEnd)
            {
                if (fillType == FillType.NewStartToEnd)
                {
                    _quadStripBuilder.Start(start, ref buffersIndexers);
                }
                _quadStripBuilder.Continue(end, ref buffersIndexers);
            }
            else
            {
                float localLerpParam = math.unlerp(lerpRange.x, lerpRange.y, _globalLerpParam);
                float3x2 middle = new float3x2(
                    math.lerp(start[0], end[0], localLerpParam),
                    math.lerp(start[1], end[1], localLerpParam)
                );
                if (fillType == FillType.NewFromStart)
                {
                    _quadStripBuilder.Start(start, ref buffersIndexers);
                    _quadStripBuilder.Continue(middle, ref buffersIndexers);
                }
                else if (fillType == FillType.NewToEnd)
                {
                    _quadStripBuilder.Start(middle, ref buffersIndexers);
                    _quadStripBuilder.Continue(end, ref buffersIndexers);
                }
                else if (fillType == FillType.ContinueFromStart)
                {
                    _quadStripBuilder.Continue(middle, ref buffersIndexers);
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Unknown QSTransSegment.ConstructType");
                }
            }
        }

        private void ConstructWithRadialType(
            in QSTS_FillData fillData,
            in float3x4 startEndLineSegs,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            RadialType radialType = fillData.Radial.Type;
            QSTSFD_Radial radialData = fillData.Radial;
            float3x2 startSeg = new float3x2(startEndLineSegs[0], startEndLineSegs[1]);
            float lerpParam = math.unlerp(fillData.LerpRange.x, fillData.LerpRange.y, _globalLerpParam);

            if (radialData.LerpLength > 1)
            {
                Debug.LogWarning("LerpLength is > 1, is this correct behaviour?");
            }

            float lerpOffset = lerpParam;// + radialData.StartLerpOffset;
            if (Between01(in lerpOffset))
            {
                float3x2 currentSeg = InterpolateRadial(lerpOffset, in fillData, startSeg);
                _quadStripBuilder.Start(currentSeg, ref buffersIndexers);
            }
            else
            {
                _quadStripBuilder.Start(startSeg, ref buffersIndexers);
            }

            float lerpDelta = radialData.LerpLength / radialData.Resolution;
            float lerpBound = lerpOffset + radialData.LerpLength;
            ClampToOne(ref lerpBound);
            for (int i = 0; i < radialData.Resolution; i++)
            {
                lerpOffset += lerpDelta;
                if (lerpOffset > lerpBound && lerpBound != 1)
                {
                    float3x2 currentSeg = InterpolateRadial(lerpBound, in fillData, startSeg);
                    DrawLineSegmentWithRaysUp(currentSeg, 1, 0.1f);
                    _quadStripBuilder.Continue(currentSeg, ref buffersIndexers);
                    return;
                }
                if (lerpOffset > 0)
                {
                    float3x2 currentSeg = InterpolateRadial(lerpOffset, in fillData, startSeg);
                    DrawLineSegmentWithRaysUp(currentSeg, 1, 0.1f);
                    _quadStripBuilder.Continue(currentSeg, ref buffersIndexers);
                }
            }
        }

        private float3x2 InterpolateRadial(float lerpOffset, in QSTS_FillData fillData, in float3x2 startSeg)
        {
            QSTSFD_Radial radialFill = fillData.Radial;
            if (radialFill.Type == RadialType.SingleRotationLerp)
            {
                float rotAngle = radialFill.AxisAngles[0].w * lerpOffset;
                quaternion q = quaternion.AxisAngle(radialFill.AxisAngles[0].xyz, rotAngle);

                return RotateLineSegmentAround(q, radialFill.Points[0], startSeg);
            }
            else if (radialFill.Type == RadialType.DoubleRotationLerp)
            {
                float2 rotAngles = new float2(
                    radialFill.AxisAngles[0].w * lerpOffset,
                    radialFill.AxisAngles[1].w * lerpOffset
                );

                quaternion q1 = quaternion.AxisAngle(radialFill.AxisAngles[0].xyz, rotAngles[0]);
                quaternion q2 = quaternion.AxisAngle(radialFill.AxisAngles[1].xyz, rotAngles[1]);

                float3x2 doubleCenters = new float3x2(radialFill.Points[0], radialFill.Points[1]);
                return RotateLineSegmentAround(q1, q2, doubleCenters, startSeg);
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("Unknown RadialConstructType");
            }
        }
    }
}