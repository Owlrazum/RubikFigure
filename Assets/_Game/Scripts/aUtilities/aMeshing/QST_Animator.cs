using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Math;

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
            in NativeArray<VertexData> vertices,
            in NativeArray<short> indices,
            in float3x2 normalUV
        )
        {
            _transition = new QS_Transition();
            _quadStripBuilder = new QuadStripBuilder(vertices, indices, normalUV);
            _globalLerpParam = -1;
        }

        public void AssignTransition(in QS_Transition transition)
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
                    float3x2 startSeg = new float3x2(startEndLineSegs[0], startEndLineSegs[1]);
                    ConstructWithRadialType(
                        in fillData,
                        in startSeg,
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
            in float3x2 startSeg,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            RadialType radialType = fillData.Radial.Type;
            QSTSFD_Radial radial = fillData.Radial;
            float lerpParam = math.unlerp(fillData.LerpRange.x, fillData.LerpRange.y, _globalLerpParam);

            if (radial.LerpLength > 1)
            {
                Debug.LogWarning("LerpLength is > 1, is this correct behaviour?");
            }

            if (radial.IsRotationLerp)
            {
                ConstructRotationLerp(in radial, in startSeg, ref buffersIndexers, lerpParam);
            }
            else if (radial.IsMoveLerp)
            {
                ConstructMoveLerp(lerpParam, in radial, in startSeg, ref buffersIndexers);
            }
        }

        private void ConstructRotationLerp(
            in QSTSFD_Radial radial,
            in float3x2 startSeg,
            ref MeshBuffersIndexers buffersIndexers,
            float lerpParam
        )
        {
            float lerpOffset = lerpParam - radial.LerpLength;
            float lerpDelta = radial.LerpLength / radial.Resolution;
            bool isStripStarted = false;
            for (int i = 0; i < radial.Resolution; i++)
            {
                lerpOffset += lerpDelta;
                if (lerpOffset > lerpParam)
                {
                    float3x2 currentSeg = InterpolateRotationLerp(lerpParam, in radial, startSeg);
                    _quadStripBuilder.Add(currentSeg, ref buffersIndexers, ref isStripStarted);
                    // DrawLineSegmentWithRaysUp(currentSeg, 1, 0.1f);
                    return;
                }
                if (lerpOffset > 0)
                {
                    float3x2 currentSeg = InterpolateRotationLerp(lerpParam, in radial, startSeg);
                    _quadStripBuilder.Add(currentSeg, ref buffersIndexers, ref isStripStarted);
                    // DrawLineSegmentWithRaysUp(currentSeg, 1, 0.1f);
                }
            }
        }

        private float3x2 InterpolateRotationLerp(float lerpParam, in QSTSFD_Radial radial, in float3x2 startSeg)
        {
            Assert.IsTrue(radial.Type == RadialType.SingleRotationLerp || radial.Type == RadialType.DoubleRotationLerp);
            switch (radial.Type)
            {
                case RadialType.SingleRotationLerp:
                    return RotateWithLerp(lerpParam, radial.AxisAngles[0], radial.Points[0], startSeg);
                case RadialType.DoubleRotationLerp:
                    float2 rotAngles = new float2(
                        radial.AxisAngles[0].w * lerpParam,
                        radial.AxisAngles[1].w * lerpParam
                    );

                    quaternion q1 = quaternion.AxisAngle(radial.AxisAngles[0].xyz, rotAngles[0]);
                    quaternion q2 = quaternion.AxisAngle(radial.AxisAngles[1].xyz, rotAngles[1]);

                    float3x2 doubleCenters = new float3x2(radial.Points[0], radial.Points[1]);
                    return RotateLineSegmentAround(q1, q2, doubleCenters, startSeg);
            }

            throw new System.ArgumentOutOfRangeException("Unknown RadialConstructType");
        }

        private void ConstructMoveLerp(
            float lerpParam,
            in QSTSFD_Radial radial,
            in float3x2 startSeg,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            float3x2 lerpedStartSeg = InterpolateStartSegMoveLerp(lerpParam, in radial, in startSeg);
            float lerpOffset = 0;
            float lerpDelta = 1 / radial.Resolution;
            bool isStripStarted = false;
            for (int i = 0; i < radial.Resolution; i++)
            {
                float3x2 currentSeg = RotateWithLerp(lerpParam, radial.AxisAngles[0], radial.Points[0], startSeg);
                _quadStripBuilder.Add(currentSeg, ref buffersIndexers, ref isStripStarted);
                lerpOffset += lerpDelta;
                ClampToOne(ref lerpOffset);
            }
        }

        private float3x2 InterpolateStartSegMoveLerp(float lerpParam, in QSTSFD_Radial radial, in float3x2 startSeg)
        {
            // clockOrder is assumed as clockwise
            float3x2 toReturn;
            switch (radial.Type)
            {
                case RadialType.MoveLerp:
                    float3 lerped = math.lerp(startSeg[0], startSeg[1], lerpParam);
                    switch (radial.VertOrder)
                    {
                        case VertOrderType.Up:
                            toReturn = new float3x2(
                                lerped,
                                startSeg[1]
                            );
                            return toReturn;
                        case VertOrderType.Down:
                            toReturn = new float3x2(
                                startSeg[0],
                                lerped
                            );
                            return toReturn;
                    }
                    break;
                case RadialType.MoveLerpWithMiddle:
                    float3 middle = radial.Points[1];
                    float3 start, end;
                    switch (radial.VertOrder)
                    {
                        case VertOrderType.Up:
                            start = math.lerp(startSeg[0], middle, lerpParam);
                            end = math.lerp(middle, startSeg[1], lerpParam);
                            break;
                        case VertOrderType.Down:
                            start = math.lerp(middle, startSeg[0], lerpParam);
                            end = math.lerp(startSeg[1], middle, lerpParam);
                            break;
                        default:
                            throw new System.ArgumentOutOfRangeException("Unknown VertOrderType");
                    }
                    toReturn = new float3x2(
                        start,
                        end
                    );
                    break;
            }

            throw new System.ArgumentOutOfRangeException("Unknown RadialType of VertOrderType");
        }

        private float3x2 RotateWithLerp(in float lerpParam, in float4 axisAngle, in float3 center, in float3x2 startSeg)
        {
            float rotAngle = axisAngle.w * lerpParam;
            quaternion q = quaternion.AxisAngle(axisAngle.xyz, rotAngle);
            return RotateLineSegmentAround(q, center, startSeg);
        }
    }
}