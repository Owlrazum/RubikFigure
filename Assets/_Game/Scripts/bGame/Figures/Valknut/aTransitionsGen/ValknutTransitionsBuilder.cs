using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Math;
using Orazum.Meshing;
using static Orazum.Math.RaysUtilities;
using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Meshing.QST_Segment;
using static Orazum.Meshing.QSTS_FillData;


public struct ValknutTransitionsBuilder
{
    private QuadStrip _origin;
    private QuadStrip _target;

    private float3x4 _originRays;
    private float3x2 _targetRay;

    private NativeArray<float> _originDistancesRatios;
    private NativeArray<float> _targetDistancesRatios;

    private float _emptyFillInDistanceRatio;
    private float3x2 _intersectionSegment;

    private NativeArray<QST_Segment> _writeBuffer;

    public ValknutTransitionsBuilder(
        in QuadStrip origin,
        in QuadStrip target
    )
    {
        _origin = origin;
        _target = target;

        _originRays = float3x4.zero;
        _targetRay = float3x2.zero;

        _originDistancesRatios = new NativeArray<float>(_origin.QuadsCount, Allocator.Temp);
        _targetDistancesRatios = new NativeArray<float>(_target.QuadsCount, Allocator.Temp);

        _emptyFillInDistanceRatio = 0;
        _intersectionSegment = float3x2.zero;

        _writeBuffer = new NativeArray<QST_Segment>();
    }

    public void InitializeOriginRays(LineEndType quadStripEnd, LineEndDirectionType raysDirection)
    {
        _originRays = _origin.GetRays(quadStripEnd, raysDirection);
        // DrawRay(_originRays[0], _originRays[1], 10, 10);
        // DrawRay(_originRays[2], _originRays[3], 10, 10);
    }
    public void InitializeTargetRay(LineEndType quadStripEnd, LineEndDirectionType raysDirection, LineEndType lineSegmentEnd)
    {
        _targetRay = _target.GetRay(quadStripEnd, raysDirection, lineSegmentEnd);
        // DrawRay(_targetRay, 10, 10);
    }

    public bool BuildTransition(
        LineEndDirectionType originDirection,
        LineEndDirectionType targetDirection,
        ref NativeArray<QST_Segment> writeBuffer)
    {
        Assert.IsTrue(writeBuffer.Length == _origin.QuadsCount + 1 + _target.QuadsCount);
        _writeBuffer = writeBuffer;
        bool areIntersecting = ComputeDistancesRatios(
            originDirection,
            targetDirection
        );
        if (!areIntersecting)
        {
            return false;
        }

        PrepareSegments(originDirection, targetDirection);
        BuildFillOutData(originDirection);
        BuildFillInData(originDirection, targetDirection);

        return true;
    }

    private bool ComputeDistancesRatios(
        LineEndDirectionType originDirection,
        LineEndDirectionType targetDirection
    )
    {
        float fillInTotalDistance = 0;
        int indexer = 0;
        if (targetDirection == LineEndDirectionType.StartToEnd)
        {
            for (int i = 0; i < _target.LineSegmentsCount - 1; i++)
            {
                float length = DistanceLineSegment(_target[i][0], _target[i + 1][0]);
                _targetDistancesRatios[indexer++] = length;
                fillInTotalDistance += length;
            }
        }
        else
        {
            for (int i = _target.LineSegmentsCount - 1; i >= 1; i--)
            {
                float length = DistanceLineSegment(_target[i][0], _target[i - 1][0]);
                _targetDistancesRatios[indexer++] = length;
                fillInTotalDistance += length;
            }
        }

        float fillOutTotalDistance = 0;
        indexer = 0;
        if (originDirection == LineEndDirectionType.StartToEnd)
        {
            for (int i = 0; i < _origin.LineSegmentsCount - 1; i++)
            {
                float length = DistanceLineSegment(_origin[i][0], _origin[i + 1][0]);
                _originDistancesRatios[indexer++] = length;
                fillOutTotalDistance += length;
            }
        }
        else
        {
            for (int i = _origin.LineSegmentsCount - 1; i >= 1; i--)
            {
                float length = DistanceLineSegment(_origin[i][0], _origin[i - 1][0]);
                _originDistancesRatios[indexer++] = length;
                fillOutTotalDistance += length;
            }
        }

        int originEndIndex = originDirection == LineEndDirectionType.StartToEnd ? _origin.LineSegmentsCount - 1 : 0;
        Intersect(originEndIndex, out float emptyZoneLength);
        if (emptyZoneLength < 0)
        {
            return false;
        }

        fillOutTotalDistance += emptyZoneLength;
        fillInTotalDistance += emptyZoneLength;

        _emptyFillInDistanceRatio = emptyZoneLength / fillInTotalDistance;

        float distance = 0;
        for (int i = 0; i < _originDistancesRatios.Length; i++)
        {
            distance += _originDistancesRatios[i];
            _originDistancesRatios[i] = distance / fillOutTotalDistance;
        }

        distance = emptyZoneLength;
        for (int i = 0; i < _targetDistancesRatios.Length; i++)
        {
            distance += _targetDistancesRatios[i];
            _targetDistancesRatios[i] = distance / fillInTotalDistance;
        }

        return true;
    }
    private void Intersect(int originEndIndex, out float emptyZoneLength)
    {
        float3x2 r1 = new float3x2(_originRays[0], _originRays[1]);
        float3x2 r2 = new float3x2(_originRays[2], _originRays[3]);
        bool intersect = IntersectSegmentToRay2D(r1, r2, _targetRay, out _intersectionSegment);
        if (!intersect)
        {
            DrawRay(r1, 10, 10);
            DrawRay(r2, 10, 10);
            DrawRay(_targetRay, 10, 10);
            UnityEngine.Debug.LogError($"No intersection");
            emptyZoneLength = -1;
            return;
        }

        float3x2 delta = _intersectionSegment - _origin[originEndIndex];
        emptyZoneLength = DistanceLineSegment(_intersectionSegment[0], _origin[originEndIndex][0]);
    }

    private void PrepareSegments(LineEndDirectionType originDirection, LineEndDirectionType targetDirection)
    {
        QST_Segment segment;
        int writeIndexer = 0;
        if (originDirection == LineEndDirectionType.StartToEnd)
        {
            for (int i = 0; i < _origin.QuadsCount; i++)
            {
                QSTS_BuilderUtils.PrepareSegment(_origin[i], _origin[i + 1], QSTS_Type.Quad,
                    fillDataLength: 0, out segment);

                _writeBuffer[writeIndexer++] = segment;
            }
        }
        else
        {
            for (int i = _origin.QuadsCount; i >= 1; i--)
            {
                QSTS_BuilderUtils.PrepareSegment(_origin[i - 1], _origin[i], QSTS_Type.Quad,
                    fillDataLength: 0, out segment);

                _writeBuffer[writeIndexer++] = segment;
            }
        }

        if (targetDirection == LineEndDirectionType.StartToEnd)
        {
            for (int i = 0; i < _target.QuadsCount; i++)
            {
                QSTS_BuilderUtils.PrepareSegment(_target[i], _target[i + 1], QSTS_Type.Quad,
                    fillDataLength: 0, out segment);

                _writeBuffer[writeIndexer++] = segment;
            }
        }
        else
        {
            for (int i = _target.QuadsCount; i >= 1; i--)
            {
                QSTS_BuilderUtils.PrepareSegment(_target[i - 1], _target[i], QSTS_Type.Quad,
                    fillDataLength: 0, out segment);

                _writeBuffer[writeIndexer++] = segment;
            }
        }

        float3x2 start = float3x2.zero;
        float3x2 end = float3x2.zero;
        if (originDirection == LineEndDirectionType.StartToEnd)
        {
            start = _origin[_origin.QuadsCount];
            end = _intersectionSegment;
        }
        else
        {
            start = _intersectionSegment;
            end = _origin[0];
        }

        QSTS_BuilderUtils.PrepareSegment(start, end, QSTS_Type.Quad,
            fillDataLength: 3, out segment);
        _writeBuffer[writeIndexer++] = segment;
    }


    private void BuildFillOutData(LineEndDirectionType originDirection)
    {
        QST_Segment current = new();
        QST_Segment next = new();

        for (int i = 0; i < _originDistancesRatios.Length; i++)
        {
            float2 fillOutLerpRange;
            if (i == 0)
            {
                current = _writeBuffer[i];
                QSTS_BuilderUtils.UpdateFillDataLength(ref current, 1);
                fillOutLerpRange = new float2(0, _originDistancesRatios[0]);
    }
            else
            {
                current = next;
                fillOutLerpRange = new float2(_originDistancesRatios[i - 1], _originDistancesRatios[i]);
            }

            QSTS_FillData fillOut = new QSTS_FillData(
                ConstructType.New,
                originDirection == LineEndDirectionType.StartToEnd ? FillType.ToEnd : FillType.ToStart,
                fillOutLerpRange
            );
            current[0] = fillOut;
            _writeBuffer[i] = current;

            int nextIndex = i + 1;
            if (nextIndex < _originDistancesRatios.Length)
            {
                next = _writeBuffer[nextIndex];
                QSTS_BuilderUtils.UpdateFillDataLength(ref next, 2);
                QSTS_FillData filledState = new QSTS_FillData(
                    ConstructType.New,
                    FillType.StartToEnd,
                    new float2(0, _originDistancesRatios[i])
                );
                next[1] = filledState;
                _writeBuffer[nextIndex] = next;
            }
        }

        QST_Segment lastSegment = _writeBuffer[_writeBuffer.Length - 1];

        float lastDistanceRatio = _originDistancesRatios[_originDistancesRatios.Length - 1];

        // fillOut
        lastSegment[0] = new QSTS_FillData(
            ConstructType.New,
            originDirection == LineEndDirectionType.StartToEnd ? FillType.FromStart : FillType.FromEnd,
            new float2(0, _emptyFillInDistanceRatio)
        );

        // filled
        lastSegment[1] = new QSTS_FillData(
            ConstructType.New,
            FillType.StartToEnd,
            new float2(_emptyFillInDistanceRatio, lastDistanceRatio)
        );

        // fillIn
        lastSegment[2] = new QSTS_FillData(
            ConstructType.New,
            originDirection == LineEndDirectionType.StartToEnd ? FillType.ToEnd : FillType.ToStart,
            new float2(lastDistanceRatio, 1)
        );
        
        _writeBuffer[_writeBuffer.Length - 1] = lastSegment;
    }

    private void BuildFillInData(LineEndDirectionType originDirection, LineEndDirectionType targetDirection)
    {
        int writeIndexer = _originDistancesRatios.Length;
        for (int i = 0; i < _target.QuadsCount; i++)
        {
            if (i != _target.QuadsCount - 1)
            { 
                QST_Segment current = _writeBuffer[writeIndexer];
                QSTS_BuilderUtils.UpdateFillDataLength(ref current, 2);

                float2 fillInLerpRange = i == 0 ? 
                    new float2(_emptyFillInDistanceRatio, _targetDistancesRatios[i]) : 
                    new float2(_targetDistancesRatios[i - 1], _targetDistancesRatios[i]);

                current[0] = new QSTS_FillData(
                    ConstructType.New,
                    targetDirection == LineEndDirectionType.StartToEnd ? FillType.FromStart: FillType.FromEnd,
                    fillInLerpRange
                );

                current[1] = new QSTS_FillData(
                    ConstructType.New, 
                    FillType.StartToEnd,
                    new float2(_targetDistancesRatios[i], 1)
                );
                _writeBuffer[writeIndexer++] = current;
            }
            else
            {
                QST_Segment current = _writeBuffer[writeIndexer];
                QSTS_BuilderUtils.UpdateFillDataLength(ref current, 1);

                current[0] = new QSTS_FillData(
                    ConstructType.New,
                    targetDirection == LineEndDirectionType.StartToEnd ? FillType.FromStart: FillType.FromEnd,
                    new float2(_targetDistancesRatios[i - 1], 1)
                );
                _writeBuffer[writeIndexer++] = current;
            }
        }
    }
 
    private void MoveLerprange(ref float2 lerpRange, float newValue)
    {
        lerpRange.x = lerpRange.y;
        lerpRange.y = newValue;
    }
}