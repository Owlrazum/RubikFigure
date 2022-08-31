using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Math.MathUtils;
using static QSTransSegment;

namespace Orazum.Meshing
{
    // QS - quad strip
    public struct QSTransition
    {
        private NativeArray<QSTransSegment>.ReadOnly _transitionSegments;
        private QuadStripBuilderVertexData _quadStripBuilder;
        private float _globalLerpParam;

        public QSTransition(
            ref NativeArray<VertexData> vertices,
            ref NativeArray<short> indices
        )
        {
            _transitionSegments = new NativeArray<QSTransSegment>.ReadOnly();
            _quadStripBuilder = new QuadStripBuilderVertexData(vertices, indices, float3x2.zero);
            _globalLerpParam = -1;
        }

        public void AssignTransitionData(
            NativeArray<QSTransSegment>.ReadOnly transitionSegments,
            in float3x2 normalAndUV
        )
        {
            _transitionSegments = transitionSegments;
            _quadStripBuilder.SetNormalAndUV(normalAndUV);
        }

        public void UpdateWithLerpPos(float globalLerpParam, ref MeshBuffersIndexers buffersIndexers)
        {
            _globalLerpParam = globalLerpParam;

            for (int i = 0; i < _transitionSegments.Length; i++)
            {
                QSTransSegment segment = _transitionSegments[i];
                for (int j = 0; j < segment.FillDataLength; j++)
                {
                    ConsiderFillData(in segment, segment[j], ref buffersIndexers);
                }
            }
        }

        private void ConsiderFillData(
            in QSTransSegment segment,
            in QSTransSegFillData fillData,
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

                if (fillData.ConstructType == MeshConstructType.Quad)
                {
                    ConstructWithQuadType(
                        in fillData,
                        in startEndLineSegs,
                        ref buffersIndexers
                    );
                }
                else if (fillData.ConstructType == MeshConstructType.Radial)
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
            in QSTransSegFillData fillData,
            in float3x4 startEndLineSegs,
            ref MeshBuffersIndexers buffersIndexers)
        {
            QuadConstructType quadType = fillData.QuadType;
            float2 lerpRange = fillData.LerpRange;
            float3x2 start = new float3x2(startEndLineSegs[0], startEndLineSegs[1]);
            float3x2 end = new float3x2(startEndLineSegs[2], startEndLineSegs[3]);

            if (quadType == QuadConstructType.NewQuadStartToEnd ||
                quadType == QuadConstructType.ContinueQuadStartToEnd)
            {
                if (quadType == QuadConstructType.NewQuadStartToEnd)
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
                if (quadType == QuadConstructType.NewQuadFromStart)
                {
                    _quadStripBuilder.Start(start, ref buffersIndexers);
                    _quadStripBuilder.Continue(middle, ref buffersIndexers);
                }
                else if (quadType == QuadConstructType.NewQuadToEnd)
                {
                    _quadStripBuilder.Start(middle, ref buffersIndexers);
                    _quadStripBuilder.Continue(end, ref buffersIndexers);
                }
                else if (quadType == QuadConstructType.ContinueQuadFromStart)
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
            in QSTransSegFillData fillData,
            in float3x4 startEndLineSegs,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            RadialConstructType constructType = fillData.RadialType;
            QSTransSegFillRadialData radialData = fillData.RadialData;
            float3x2 startSeg = new float3x2(startEndLineSegs[0], startEndLineSegs[1]);
            float lerpParam = math.unlerp(fillData.LerpRange.x, fillData.LerpRange.y, _globalLerpParam);

            if (radialData.LerpLength > 1)
            {
                Debug.LogWarning("LerpLength is > 1, is this correct behaviour?");
            }

            float lerpOffset = lerpParam + radialData.StartLerpOffset;
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

        private float3x2 InterpolateRadial(float lerpOffset, in QSTransSegFillData fillData, in float3x2 startSeg)
        {
            Assert.IsTrue(fillData.ConstructType == MeshConstructType.Radial);
            QSTransSegFillRadialData radialData = fillData.RadialData;
            if (fillData.RadialType == RadialConstructType.Single)
            {
                float rotAngle = radialData.AxisAngles[0].w * lerpOffset;
                quaternion q = quaternion.AxisAngle(radialData.AxisAngles[0].xyz, rotAngle);

                return RotateLineSegmentAround(q, radialData.Centers[0], startSeg);
            }
            else if (fillData.RadialType == RadialConstructType.Double)
            {
                float2 rotAngles = new float2(
                    radialData.AxisAngles[0].w * lerpOffset,
                    radialData.AxisAngles[1].w * lerpOffset
                );

                quaternion q1 = quaternion.AxisAngle(radialData.AxisAngles[0].xyz, rotAngles[0]);
                quaternion q2 = quaternion.AxisAngle(radialData.AxisAngles[1].xyz, rotAngles[1]);

                return RotateLineSegmentAround(q1, q2, radialData.Centers, startSeg);
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("Unknown RadialConstructType");
            }
        }
    }
}