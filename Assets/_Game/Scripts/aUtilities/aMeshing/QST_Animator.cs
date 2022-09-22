using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using static Orazum.Constants.Math;
using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Math.MathUtils;
using static Orazum.Meshing.QST_Segment;
using static Orazum.Meshing.QSTS_FillData;
using static Orazum.Meshing.QSTSFD_Radial;

namespace Orazum.Meshing
{
    public struct QST_Animator
    {
        [ReadOnly]
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
                if (segment.Type == QSTS_Type.Quad)
                {
                    ConstructWithQuadType(
                        in fillData,
                        segment.StartLineSegment,
                        segment.EndLineSegment,
                        ref buffersIndexers
                    );
                }
                else if (segment.Type == QSTS_Type.Radial)
                {
                    ConstructWithRadialType(
                        in fillData,
                        segment.StartLineSegment,
                        segment.EndLineSegment,
                        ref buffersIndexers
                    );
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Unknown MeshConstructType");
                }
            }
        }

        #region QuadType
        private void ConstructWithQuadType(
            in QSTS_FillData fillData,
            float3x2 start,
            float3x2 end,
            ref MeshBuffersIndexers buffersIndexers)
        {
            FillType fillType = fillData.Fill;
            float2 lerpRange = fillData.LerpRange;

            float localLerpParam = math.unlerp(lerpRange.x, lerpRange.y, _globalLerpParam);
            float3x2 middle = new float3x2(
                math.lerp(start[0], end[0], localLerpParam),
                math.lerp(start[1], end[1], localLerpParam)
            );

            FillQuadType(fillData, start, end, middle, ref buffersIndexers);
        }

        private void FillQuadType(
            QSTS_FillData fillData,
            in float3x2 start,
            in float3x2 end,
            in float3x2 middle,
            ref MeshBuffersIndexers buffersIndexers)
        {
            FillType fillType = fillData.Fill;
            ConstructType constructType = fillData.Construct;
            switch (fillType)
            {
                case FillType.StartToEnd:
                    StartStripIfNeeded(start, constructType, ref buffersIndexers);
                    _quadStripBuilder.Continue(end, ref buffersIndexers);
                    break;
                case FillType.FromStart:
                    StartStripIfNeeded(start, constructType, ref buffersIndexers);
                    _quadStripBuilder.Continue(middle, ref buffersIndexers);
                    break;
                case FillType.ToEnd:
                    if (constructType == ConstructType.Continue)
                    {
                        Debug.LogWarning("ToX fillType with Continue ConstructType may lead to not wanted results.");
                    }
                    StartStripIfNeeded(middle, constructType, ref buffersIndexers);
                    _quadStripBuilder.Continue(end, ref buffersIndexers);
                    break;
                case FillType.FromEnd:
                    // negative direction, we should flipped line segments
                    float3x2 flippedEnd = new float3x2(end[1], end[0]);
                    StartStripIfNeeded(flippedEnd, constructType, ref buffersIndexers);
                    float3x2 flippedMiddle = new float3x2(middle[1], middle[0]);
                    _quadStripBuilder.Continue(flippedMiddle, ref buffersIndexers);
                    break;
                case FillType.ToStart:
                    // negative direction, we should flipped line segments
                    flippedMiddle = new float3x2(middle[1], middle[0]);
                    if (constructType == ConstructType.Continue)
                    {
                        Debug.LogWarning("ToX fillType with Continue ConstructType may lead to not wanted results.");
                    }
                    StartStripIfNeeded(flippedMiddle, constructType, ref buffersIndexers);
                    float3x2 flippedStart = new float3x2(start[1], start[0]);
                    _quadStripBuilder.Continue(flippedStart, ref buffersIndexers);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException("Unknown FillType");
            }
        }

        private void StartStripIfNeeded(float3x2 lineSegment, ConstructType constructType, ref MeshBuffersIndexers buffersIndexers)
        {
            if (constructType == ConstructType.New)
            {
                _quadStripBuilder.Start(lineSegment, ref buffersIndexers);
            }
        }
        #endregion

        #region RadialType
        private void ConstructWithRadialType(
            in QSTS_FillData fillData,
            float3x2 start,
            float3x2 end,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            Assert.IsTrue(fillData.Radial.Resolution > 1);

            QSTSFD_Radial radial = fillData.Radial;
            float lerpParam = math.unlerp(fillData.LerpRange.x, fillData.LerpRange.y, _globalLerpParam);

            if (radial.MaxLerpLength > 1)
            {
                Debug.LogWarning("LerpLength is > 1, is this correct behaviour?");
            }

            if (radial.Type == RadialType.FirstOrderRotation)
            {
                Construct_SRL(fillData, start, end, lerpParam, ref buffersIndexers);
            }
            else if (radial.Type == RadialType.Move)
            {
                ConstructMoveLerp(fillData, start, end, lerpParam, ref buffersIndexers);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Unknown RadialType");
            }
        }

        #region SingleRotationLerp
        private void Construct_SRL(
            in QSTS_FillData fillData,
            in float3x2 start,
            in float3x2 end,
            in float lerpParam,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            NativeArray<float3x2> segs = GetLerpSegs_SRL(fillData, start, end, lerpParam);

            FillType fillType = fillData.Fill;
            ConstructType constructType = fillData.Construct;
            if ((fillType == FillType.ToEnd || fillType == FillType.ToStart) && constructType == ConstructType.Continue)
            {
                Debug.LogWarning("ToX fillType with Continue ConstructType may lead to not wanted results.");
            }
            StartStripIfNeeded(segs[0], constructType, ref buffersIndexers);
            for (int i = 1; i < segs.Length; i++)
            {
                _quadStripBuilder.Continue(segs[i], ref buffersIndexers);
            }
        }

        private NativeArray<float3x2> GetLerpSegs_SRL(
            in QSTS_FillData fillData,
            in float3x2 start,
            in float3x2 end,
            in float lerpParamInput
        )
        {
            QSTSFD_Radial radial = fillData.Radial;
            FillType fillType = fillData.Fill;

            float lerpDelta = radial.MaxLerpLength / radial.Resolution;
            float lerpParam = lerpParamInput;
            FillTypeLerpConstruct lerpConstruct = new FillTypeLerpConstruct(fillData, radial.MaxLerpLength, ref lerpParam);

            NativeArray<float3x2> segs = new NativeArray<float3x2>(lerpConstruct.SegsCount, Allocator.Temp);
            int indexer = 0;
            if (lerpConstruct.AddStart > 0)
            {
                segs[indexer++] = start;
            }

            float3x2 lerpSeg = RotateLerp_SRL(radial, start, lerpParam);
            if (lerpConstruct.AddLerpAtStart)
            {
                segs[indexer++] = lerpSeg;
            }

            float2 lerpOffset = lerpConstruct.LerpOffset;
            for (int i = 0; i < lerpConstruct.DeltaSegsCount; i++)
            {
                lerpOffset.x += lerpDelta;
                if (lerpOffset.x >= 1)
                {
                    break;
                }
                if (lerpOffset.x < lerpOffset.y)
                {
                    segs[indexer++] = RotateLerp_SRL(radial, start, lerpOffset.x);
                }
            }

            if (lerpConstruct.AddLerpAtEnd)
            {
                segs[indexer++] = lerpSeg;
            }

            if (lerpConstruct.AddEnd > 0)
            {
                segs[indexer++] = end;
            }
            return segs;
        }

        private float3x2 RotateLerp_SRL(
            QSTSFD_Radial radial,
            in float3x2 rotationSegment,
            in float lerpParam
        )
        {
            quaternion q = quaternion.AxisAngle(
                radial.PrimaryAxisAngle.xyz,
                radial.PrimaryAxisAngle.w * lerpParam
            );
            return RotateLineSegmentAround(q, radial.RotationCenter, rotationSegment);
        }
        #endregion // SingleRotationLerp

        #region MoveLerp
        private void ConstructMoveLerp(
            in QSTS_FillData fillData,
            in float3x2 start,
            in float3x2 end,
            float lerpParam,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            QSTSFD_Radial radial = fillData.Radial;
            float3x2 first = MoveLerpStartSeg(fillData.Fill, start, lerpParam);
            QuadStrip quadStrip = GetRadialQuadStrip(radial, first);
            _quadStripBuilder.Build(quadStrip, ref buffersIndexers);
        }

        private float3x2 MoveLerpStartSeg(
            in FillType fillType,
            in float3x2 start,
            float lerpParam
        )
        {
            if (fillType == FillType.FromEnd || fillType == FillType.ToStart)
            {
                lerpParam = 1 - lerpParam;
            }

            float3 middle = math.lerp(start[0], start[1], lerpParam);
            switch (fillType)
            {
                case FillType.FromStart:
                case FillType.ToStart:
                    return new float3x2(start[0], middle);
                case FillType.FromEnd:
                case FillType.ToEnd:
                    return new float3x2(middle, start[1]);
                default:
                    throw new ArgumentOutOfRangeException($"This fillType {fillType} is not supported for DoubleMoveLerp");
            }
        }

        #endregion // MoveLerp

        // SingleRotation
        private QuadStrip GetRadialQuadStrip(
            in QSTSFD_Radial radial,
            in float3x2 rotationSegment
        )
        {
            float lerpOffset = 0;
            float lerpDelta = 1.0f / radial.Resolution;

            NativeArray<float3x2> lineSegments = new NativeArray<float3x2>(radial.Resolution + 1, Allocator.Temp);
            int indexer = 0;
            for (int i = 0; i < radial.Resolution + 1; i++)
            {
                lineSegments[indexer++] = RotateLerp_SRL(radial, rotationSegment, lerpOffset);
                lerpOffset += lerpDelta;
                ClampToOne(ref lerpOffset);
            }

            return new QuadStrip(lineSegments);
        }
        #endregion // RadialType
    }
}