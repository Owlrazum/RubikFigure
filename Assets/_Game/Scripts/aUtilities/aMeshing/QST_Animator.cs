using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Math;
using Orazum.Collections;

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
            RadialType radialType = fillData.Radial.Type;
            QSTSFD_Radial radial = fillData.Radial;
            float lerpParam = math.unlerp(fillData.LerpRange.x, fillData.LerpRange.y, _globalLerpParam);

            if (radial.LerpLength > 1)
            {
                Debug.LogWarning("LerpLength is > 1, is this correct behaviour?");
            }

            if (radial.IsRotationLerp)
            {
                ConstructRotationLerp(fillData, start, end, lerpParam, ref buffersIndexers);
            }
            else if (radial.IsMoveLerp)
            {
                ConstructMoveLerp(fillData, start, end, lerpParam, ref buffersIndexers);
            }
        }

        #region RotationLerp
        private void ConstructRotationLerp(
            in QSTS_FillData fillData,
            in float3x2 start,
            in float3x2 end,
            float lerpParam,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            QSTSFD_Radial radial = fillData.Radial;
            float lerpOffset = lerpParam - radial.LerpLength;
            float lerpDelta = radial.LerpLength / radial.Resolution;

            Assert.IsTrue(radial.Resolution > 1);
            Assert.IsTrue(radial.IsRotationLerp);

            int maxMiddlesCount = 0;
            if (lerpOffset < 0)
            {
                float lerpAmount = lerpOffset + radial.LerpLength;
                maxMiddlesCount = (int)(lerpAmount / lerpDelta + 1);
                Debug.Log($"{lerpAmount:F2} {lerpAmount / lerpDelta + 1:F2} {maxMiddlesCount}");
            }
            else
            {
                maxMiddlesCount = radial.Resolution;
            }

            NativeList<float3x2> middles = new NativeList<float3x2>(maxMiddlesCount, Allocator.Temp);
            for (int i = 0; i < radial.Resolution; i++)
            {
                lerpOffset += lerpDelta;
                if (lerpOffset > lerpParam)
                {
                    if (lerpParam != 1)
                    {
                        float3x2 rotationMiddle = GetRotationMiddle(fillData, start, end, lerpOffset);
                        middles.Add(rotationMiddle);
                    }
                    break;
                }
                if (lerpOffset > 0)
                {
                    float3x2 rotationMiddle = GetRotationMiddle(fillData, start, end, lerpOffset);
                    middles.Add(rotationMiddle);
                }
            }

            FillRadialTypeRotationLerp(fillData, start, end, middles, ref buffersIndexers);
        }

        private float3x2 GetRotationMiddle(
            in QSTS_FillData fillData,
            in float3x2 start,
            in float3x2 end,
            float lerpParam
        )
        {
            QSTSFD_Radial radial = fillData.Radial;

            float2 angles;
            if (radial.Type == RadialType.SingleRotation)
            {
                angles = new float2(radial.AxisAngles[0].w * lerpParam, 0);
            }
            else if (radial.Type == RadialType.DoubleRotation)
            {
                angles = new float2(
                    radial.AxisAngles[0].w * lerpParam,
                    radial.AxisAngles[1].w * lerpParam
                );
            }
            else
            {
                throw new ArgumentOutOfRangeException("Unsupported RadialType");
            }

            float3x2 rotationSegment;
            float angleSign = 0;
            switch (fillData.Fill)
            {
                case FillType.FromStart:
                case FillType.ToEnd:
                    rotationSegment = start;
                    angleSign = 1;
                    break;
                case FillType.FromEnd:
                case FillType.ToStart:
                    rotationSegment = end;
                    angleSign = -1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"This fillType {fillData.Fill} is not supported for RotationLerp");
            }
            angles *= angleSign;

            switch (radial.Type)
            {
                case RadialType.SingleRotation:
                    float3 center = radial.Points[0];
                    float3 axis = radial.AxisAngles[0].xyz;
                    return RotateSingle(rotationSegment, center, axis, angles.x);
                case RadialType.DoubleRotation:

                    lerpParam = 1 - lerpParam;
                    float3x2 axises = new float3x2(
                        radial.AxisAngles[0].xyz,
                        radial.AxisAngles[1].xyz
                    );

                    float3x2 centers = new float3x2(radial.Points[0], radial.Points[1]);
                    return RotateDouble(rotationSegment, centers, axises, angles);
            }

            throw new System.ArgumentOutOfRangeException("Unknown RadialConstructType");
        }

        private float3x2 RotateSingle(in float3x2 start, in float3 center, in float3 axis, float angle)
        {
            quaternion q = quaternion.AxisAngle(axis, angle);
            return RotateLineSegmentAround(q, center, start);
        }
        private float3x2 RotateDouble(in float3x2 start, in float3x2 centers, in float3x2 axises, float2 angles)
        {
            quaternion q1 = quaternion.AxisAngle(axises[0], angles[0]);
            quaternion q2 = quaternion.AxisAngle(axises[1], angles[1]);
            return RotateLineSegmentAround(q1, q2, centers, start);
        }

        private void FillRadialTypeRotationLerp(
            QSTS_FillData fillData,
            in float3x2 start,
            in float3x2 end,
            in NativeList<float3x2> middles,
            ref MeshBuffersIndexers buffersIndexers
        )
        {
            FillType fillType = fillData.Fill;
            ConstructType constructType = fillData.Construct;
            switch (fillType)
            { 
                case FillType.FromStart:
                    StartStripIfNeeded(start, constructType, ref buffersIndexers);
                    for (int i = 0; i < middles.Count; i++)
                    { 
                        _quadStripBuilder.Continue(middles[i], ref buffersIndexers);
                    }
                    break;
                case FillType.ToEnd:
                    if (constructType == ConstructType.Continue)
                    {
                        Debug.LogWarning("ToX fillType with Continue ConstructType may lead to not wanted results.");
                    }
                    StartStripIfNeeded(middles[0], constructType, ref buffersIndexers);
                    for (int i = 1; i < middles.Count; i++)
                    {
                        _quadStripBuilder.Continue(middles[i], ref buffersIndexers);
                    }
                    _quadStripBuilder.Continue(end, ref buffersIndexers);
                    break;
                case FillType.FromEnd:
                    float3x2 flippedMiddle;
                    // negative direction, we should flipped line segments and reverse middles
                    CollectionUtilities.ReverseNativeList(middles);

                    float3x2 flippedEnd = new float3x2(end[1], end[0]);
                    StartStripIfNeeded(flippedEnd, constructType, ref buffersIndexers);
                    for (int i = 0; i < middles.Count; i++)
                    { 
                        flippedMiddle = new float3x2(middles[i][1], middles[i][0]);
                        _quadStripBuilder.Continue(flippedMiddle, ref buffersIndexers);
                    }
                    break;
                case FillType.ToStart:
                    // negative direction, we should flipped line segments and reverse middles
                    CollectionUtilities.ReverseNativeList(middles);

                    flippedMiddle = new float3x2(middles[0][1], middles[0][0]);
                    if (constructType == ConstructType.Continue)
                    {
                        Debug.LogWarning("ToX fillType with Continue ConstructType may lead to not wanted results.");
                    }
                    StartStripIfNeeded(flippedMiddle, constructType, ref buffersIndexers);

                    for (int i = 1; i < middles.Count; i++)
                    { 
                        flippedMiddle = new float3x2(middles[i][1], middles[i][0]);
                        _quadStripBuilder.Continue(flippedMiddle, ref buffersIndexers);    
                    }
                    float3x2 flippedStart = new float3x2(start[1], start[0]);
                    _quadStripBuilder.Continue(flippedStart, ref buffersIndexers);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException($"This fillType {fillType} is not supported for RotationLerp");
            }
        }
        #endregion // RadialLerp

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
            float lerpOffset = 0;
            float lerpDelta = 1 / radial.Resolution;
            bool isStripStarted = false;

            float3 center = radial.Points[0];
            float3 axis = radial.AxisAngles[0].xyz;
            float angle = 0;
            for (int i = 0; i < radial.Resolution; i++)
            {
                angle = lerpOffset * radial.AxisAngles[0].w;
                float3x2 currentSeg = RotateSingle(start, center, axis, angle);
                _quadStripBuilder.Add(currentSeg, ref buffersIndexers, ref isStripStarted);
                lerpOffset += lerpDelta;
                ClampToOne(ref lerpOffset);
            }
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
        
        #endregion // RadialType
    }
}