using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using static Orazum.Constants.Math;
using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Math.MathUtils;
using static QST_Segment;
using static QSTS_FillData;
using static QSTSFD_Radial;

namespace Orazum.Meshing
{
    public struct QST_Animator
    {
        [ReadOnly]
        private QS_Transition _transition;

        private QuadStripBuilder _quadStripBuilder;
        private QuadGridBuilder _gridBuilder;
        private float _globalLerpParam;

        public QST_Animator(
            in NativeArray<VertexData> vertices,
            in NativeArray<short> indices,
            in float3x2 normalUV
        )
        {
            _transition = new QS_Transition();
            _quadStripBuilder = new QuadStripBuilder(vertices, indices, normalUV);
            _gridBuilder = new QuadGridBuilder(vertices, indices, normalUV[1].xy);
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

            if (radial.Type == RadialType.SingleRotation)
            {
                Construct_SRL(fillData, start, end, lerpParam, ref buffersIndexers);
            }
            else if (radial.Type == RadialType.DoubleRotation)
            {
                Construct_DRL(fillData, start, end, lerpParam, ref buffersIndexers);
            }
            else if (radial.IsMoveLerp)
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
            FillTypeLerpConstruct lerpConstruct = new FillTypeLerpConstruct(fillType, radial.MaxLerpLength, ref lerpParam);
            int segsCount = lerpConstruct.GetSegsCount(lerpDelta, out int deltaSegsCount);

            NativeArray<float3x2> segs = new NativeArray<float3x2>(segsCount, Allocator.Temp);
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
            for (int i = 0; i < deltaSegsCount; i++)
            {
                lerpOffset.x += lerpDelta;
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

        #region DoubleRotationLerp

        private void Construct_DRL(
            in QSTS_FillData fillData,
            in float3x2 start,
            in float3x2 end,
            in float lerpParamInput,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            QuadStrip quadStrip = GetRadialQuadStrip(fillData.Radial, start);

            float lerpParam = lerpParamInput;
            var lerpConstruct = new FillTypeLerpConstruct(fillData.Fill, fillData.Radial.MaxLerpLength, ref lerpParam);

            QSTSFD_Radial radial = fillData.Radial;
            float lerpDelta = radial.MaxLerpLength / radial.Resolution;
            int segsCount = lerpConstruct.GetSegsCount(lerpDelta, out int deltaSegsCount);
            NativeArray<float3> lerpPoints = new NativeArray<float3>(segsCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            NativeArray<float3> dim = UpdateLerpedPoints_DRL(
                radial, 
                quadStrip[0], 
                lerpConstruct, 
                lerpDelta, lerpParam, 
                ref lerpPoints
            );

            // DrawGridDim(dim, 10);

            _gridBuilder.Start(dim, ref buffersIndexers);
            for (int i = 1; i < quadStrip.LineSegmentsCount; i++)
            {
                dim = UpdateLerpedPoints_DRL(
                    radial, 
                    quadStrip[i], 
                    lerpConstruct, 
                    lerpDelta, lerpParam, 
                    ref lerpPoints
                );
                // DrawGridDim(dim, 10);

                _gridBuilder.Continue(dim, ref buffersIndexers);
            }
        }

        private NativeArray<float3> UpdateLerpedPoints_DRL(
            in QSTSFD_Radial radial,
            in float3x2 lineSegment,
            in FillTypeLerpConstruct lerpConstruct,
            in float lerpDelta,
            in float lerpParam,
            ref NativeArray<float3> lerpPoints
        )
        {
            float3 start = lineSegment[0];
            float3 end = lineSegment[1];
            quaternion perp = quaternion.AxisAngle(math.up(), TAU / 4);
            float3 axis = GetDirection(perp, lineSegment);
            float3 center = GetLineSegmentCenter(lineSegment);
            int indexer = 0;
            if (lerpConstruct.AddStart > 0)
            {
                lerpPoints[indexer++] = start;
            }

            float angle = radial.SecondaryAngle * lerpParam;
            quaternion q = quaternion.AxisAngle(axis, angle);

            float3 lerpPoint = RotateAround(start, center, q);
            if (lerpConstruct.AddLerpAtStart)
            {
                lerpPoints[indexer++] = lerpPoint;
            }

            int deltaSegsCount = lerpConstruct.GetDeltaSegsCount(lerpDelta);
            float2 lerpOffset = lerpConstruct.LerpOffset;
            for (int i = 0; i < deltaSegsCount; i++)
            {
                lerpOffset.x += lerpDelta;
                if (lerpOffset.x < lerpOffset.y)
                {
                    angle = radial.SecondaryAngle * lerpOffset.x;
                    q = quaternion.AxisAngle(axis, angle);
                    lerpPoints[indexer++] = RotateAround(start, center, q);
                }
            }

            if (lerpConstruct.AddLerpAtEnd)
            {
                lerpPoints[indexer++] = lerpPoint;
            }

            if (lerpConstruct.AddEnd > 0)
            {
                lerpPoints[indexer++] = end;
            }
            return lerpPoints;
        }
        #endregion

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
            float3x2 lerpedStartSeg = InterpolateStartSegMoveLerp(radial.Type, fillData.Fill, start, end, lerpParam);

            QuadStrip quadStrip = GetRadialQuadStrip(radial, lerpedStartSeg);
            _quadStripBuilder.Build(quadStrip, ref buffersIndexers);
        }

        private float3x2 InterpolateStartSegMoveLerp(
            RadialType radialType,
            FillType fillType,
            in float3x2 start,
            in float3x2 end,
            float lerpParam
        )
        {
            switch (radialType)
            {
                case RadialType.SingleMove:
                    return MoveSingle(fillType, start, lerpParam);
                case RadialType.DoubleMove:
                    float3x4 ranges = new float3x4(
                        start[0], start[1],
                        end[0], end[1]
                    );
                    return MoveDouble(fillType, ranges, lerpParam);
                default:
                    throw new System.ArgumentOutOfRangeException("Unknown RadialType Move");
            }
        }
        private float3x2 MoveSingle(FillType fillType, in float3x2 range, float lerpParam)
        {
            switch (fillType)
            {
                case FillType.FromStart:
                    float3 middlePlus = math.lerp(range[0], range[1], lerpParam);
                    return new float3x2(range[0], middlePlus);
                case FillType.ToEnd:
                    middlePlus = math.lerp(range[0], range[1], lerpParam);
                    return new float3x2(middlePlus, range[1]);
                case FillType.ToStart:
                    float3 middleMinus = math.lerp(range[1], range[0], lerpParam);
                    return new float3x2(range[0], middleMinus);
                case FillType.FromEnd:
                    middleMinus = math.lerp(range[1], range[0], lerpParam);
                    return new float3x2(middleMinus, range[1]);
                default:
                    throw new ArgumentOutOfRangeException($"This fillType {fillType} is not supported for SingleMoveLerp");
            }
        }

        private float3x2 MoveDouble(FillType fillType, in float3x4 ranges, float lerpParam)
        {
            // The notion of FillType is collapsingTwice regarding direction.
            // if different behaviour is needed, consider implementing it yourself.
            switch (fillType)
            {
                case FillType.FromStart:
                case FillType.ToEnd:
                    return new float3x2(
                        math.lerp(ranges[0], ranges[1], lerpParam),
                        math.lerp(ranges[2], ranges[3], lerpParam)
                    );
                case FillType.FromEnd:
                case FillType.ToStart:
                    return new float3x2(
                        math.lerp(ranges[1], ranges[0], lerpParam),
                        math.lerp(ranges[3], ranges[2], lerpParam)
                    );
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
            for (int i = 0; i < radial.Resolution; i++)
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