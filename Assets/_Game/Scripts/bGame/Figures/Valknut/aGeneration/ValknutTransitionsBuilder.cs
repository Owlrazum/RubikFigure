
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using static Orazum.Math.MathUtilities;
using static QSTransSegment;

public struct ValknutTransitionsBuilder
{
    private enum TransitionType
    {
        TasToTas,
        TasToOas,
        OasToTas,
        OasToOas
    }
    private TransitionType _transitionType;

    private QuadStrip _origin;
    private QuadStrip _target;

    private NativeArray<QSTransSegment> _transSegments;
    private NativeArray<float2x4> _startEndSegs;
    private NativeArray<float> _outFillDistances;
    private NativeArray<float> _inFillDistances;

    private int2 _transSegmentsIndexer;
    private float _emptyZoneLength;

    public ValknutTransitionsBuilder(
        in QuadStrip origin,
        in QuadStrip target,
        ref NativeArray<QSTransSegment> toBuild
    )
    {
        _origin = origin;
        _target = target;
        _transSegments = toBuild;

        _transSegmentsIndexer = new int2(0, _origin.QuadsCount + 1 + _target.QuadsCount);
        _startEndSegs = new NativeArray<float2x4>(_transSegmentsIndexer.y, Allocator.Temp);

        _outFillDistances = new NativeArray<float>(_origin.QuadsCount, Allocator.Temp);
        _inFillDistances = new NativeArray<float>(_target.QuadsCount, Allocator.Temp);
        _emptyZoneLength = 0;

        if (_origin.QuadsCount == 3)
        {
            if (target.QuadsCount == 3)
            {
                _transitionType = TransitionType.TasToTas;
                return;
            }
            else if (target.QuadsCount == 2)
            {
                _transitionType = TransitionType.TasToOas;
                return;
            }
        }
        else if (_origin.QuadsCount == 2)
        {
            if (target.QuadsCount == 3)
            {
                _transitionType = TransitionType.OasToTas;
                return;
            }
            else if (target.QuadsCount == 2)
            {
                _transitionType = TransitionType.OasToOas;
                return;
            }
        }

        throw new System.ArgumentOutOfRangeException("QuadStrips quadsCount should be in [2, 3]");
    }

    public void BuildTransitionData()
    {
        ComputeDistancesAndSegs(
            out float2 fillInOutTotalDistances
        );
        BuildFillOutData(in fillInOutTotalDistances.y, out QSTransSegment lastSegment);
        BuildFillInData(in fillInOutTotalDistances.x, ref lastSegment);

        _startEndSegs.Dispose();
        _outFillDistances.Dispose();
        _inFillDistances.Dispose();
    }

    private void ComputeDistancesAndSegs(
        out float2 fillInOutTotalDistances
    )
    {
        fillInOutTotalDistances = float2.zero;

        int outDistancesIndexer = 0;

        int3 index = int3.zero;
        GetIndex(_origin.QuadsCount, out index);
        for (int i = index.x; index.z > 0 ? i < index.z : i > index.z; i += index.y)
        {
            float2x2 startSeg = new float2x2(_origin[i][0], _origin[i][1]);
            float2x2 endSeg = new float2x2(_origin[i + index.y][0], _origin[i + index.y][1]);
            _startEndSegs[_transSegmentsIndexer.x++] = new float2x4(startSeg[0], startSeg[1], endSeg[0], endSeg[1]);

            float2x2 delta = _origin[i + index.y] - _origin[i];
            _outFillDistances[outDistancesIndexer] = math.length(delta[0]);
            fillInOutTotalDistances.y += _outFillDistances[outDistancesIndexer];
            outDistancesIndexer++;
        }

        GetIndex(_origin.QuadsCount + 1, out index);
        ComputeDistsPosInEmptyZone(ref index, ref fillInOutTotalDistances);

        int inDistancesIndexer = 0;
        GetIndex(_target.QuadsCount, out index);
        for (int i = index.x; index.z > 0 ? i < index.z : i > index.z; i += index.y)
        {
            float2x2 startSeg = new float2x2(_target[i][0], _target[i][1]);
            float2x2 endSeg = new float2x2(_target[i + index.y][0], _target[i + index.y][1]);
            _startEndSegs[_transSegmentsIndexer.x++] = new float2x4(startSeg[0], startSeg[1], endSeg[0], endSeg[1]);

            float2x2 delta = _target[i + index.y] - _target[i];
            _inFillDistances[inDistancesIndexer] = math.length(delta[0]);
            fillInOutTotalDistances.x += _inFillDistances[inDistancesIndexer];
            inDistancesIndexer++;
        }

        _transSegmentsIndexer.x = 0;
    }
    private void ComputeDistsPosInEmptyZone(ref int3 index, ref float2 fillInOutTotalDistances)
    {
        GetIntersectionRays(out float4x2 originRays, out float4x2 targetRays);
        bool intersect = IntersectSegmentRays(originRays, targetRays, out float2x2 intersectSegment);
        Assert.IsTrue(intersect);

        int lastIndex = index.z - index.y;

        float2x2 startSeg = new float2x2(_origin[lastIndex][0], _origin[lastIndex][1]);
        float2x2 endSeg = new float2x2(intersectSegment[0], intersectSegment[1]);
        _startEndSegs[_transSegmentsIndexer.x++] = new float2x4(startSeg[0], startSeg[1], endSeg[0], endSeg[1]);

        float2x2 delta = intersectSegment - _origin[lastIndex];
        _emptyZoneLength = math.length(delta[0]);
        fillInOutTotalDistances.x += _emptyZoneLength;
        fillInOutTotalDistances.y += _emptyZoneLength;
    }
    private void GetIntersectionRays(out float4x2 originRays, out float4x2 targetRays)
    {
        switch (_transitionType)
        {
            case TransitionType.TasToTas:
                originRays = _origin.GetRays(LineEndType.End, LineEndDirectionType.StartToEnd);
                targetRays = _target.GetRays(LineEndType.Start, LineEndDirectionType.EndToStart);
                return;
            case TransitionType.TasToOas:
                originRays = _origin.GetRays(LineEndType.Start, LineEndDirectionType.EndToStart);
                targetRays = _target.GetRays(LineEndType.End, LineEndDirectionType.StartToEnd);
                return;
            case TransitionType.OasToTas:
                originRays = _origin.GetRays(LineEndType.Start, LineEndDirectionType.EndToStart);
                targetRays = _target.GetRays(LineEndType.Start, LineEndDirectionType.EndToStart);
                return;
            case TransitionType.OasToOas:
                originRays = _origin.GetRays(LineEndType.End, LineEndDirectionType.StartToEnd);
                targetRays = _target.GetRays(LineEndType.End, LineEndDirectionType.StartToEnd);
                return;
        }

        throw new System.Exception();
    }
    private void GetIndex(int upperBound, out int3 index)
    {
        if (_transitionType == TransitionType.TasToTas || _transitionType == TransitionType.OasToOas)
        {
            index.x = 0;
            index.y = 1;
            index.z = upperBound;
        }
        else
        {
            index.x = upperBound - 1;
            index.y = -1;
            index.z = 0;
        }
    }

    private void BuildFillOutData(
        in float outFillTotalDistance,
        out QSTransSegment lastFillOutSegment
    )
    {
        _transSegmentsIndexer.x = 0;
        // prev current distance ratios
        float2 dr = new float2(-1, _outFillDistances[_transSegmentsIndexer.x] / outFillTotalDistance);

        for (int i = 0; i < _origin.QuadsCount; i++)
        {
            float2x4 startEndSeg = _startEndSegs[_transSegmentsIndexer.x];
            float2x2 startLineSeg = new float2x2(startEndSeg[0], startEndSeg[1]);
            float2x2 endLineSeg = new float2x2(startEndSeg[2], startEndSeg[3]);

            if (i == 0)
            {
                QSTransSegment firstSegment = new QSTransSegment(startLineSeg, endLineSeg, fillDataLength: 1);
                firstSegment[0] = new QSTransSegFillData(
                    new float2(0, dr.y),
                    QuadConstructType.NewQuadToEnd
                );
                _transSegments[_transSegmentsIndexer.x++] = firstSegment;
            }
            else
            {
                QSTransSegment segment = new QSTransSegment(startLineSeg, endLineSeg, fillDataLength: 2);
                QSTransSegFillData filledState = new QSTransSegFillData(
                    new float2(0, dr.x),
                    QuadConstructType.ContinueQuadStartToEnd
                );
                QSTransSegFillData fillingOutState = new QSTransSegFillData(
                    new float2(dr.x, dr.y),
                    QuadConstructType.NewQuadToEnd
                );
                segment[0] = filledState;
                segment[1] = fillingOutState;
                _transSegments[_transSegmentsIndexer.x++] = segment;
            }

            dr.x = dr.y;
            if (_transSegmentsIndexer.x < _origin.QuadsCount)
            { 
                dr.y += _outFillDistances[_transSegmentsIndexer.x] / outFillTotalDistance;
            }
        }

        float2x4 lastStartEndSeg = _startEndSegs[_transSegmentsIndexer.x];
        float2x2 lastStartLineSeg = new float2x2(lastStartEndSeg[0], lastStartEndSeg[1]);
        float2x2 lastEndLineSeg = new float2x2(lastStartEndSeg[2], lastStartEndSeg[3]);
        lastFillOutSegment = new QSTransSegment(lastStartLineSeg, lastEndLineSeg, 3);
        QSTransSegFillData lastSegFilledState = new QSTransSegFillData(
                    new float2(0, dr.x),
                    QuadConstructType.ContinueQuadStartToEnd
                );
        QSTransSegFillData lastSegFillingOutState = new QSTransSegFillData(
            new float2(dr.x, 1),
            QuadConstructType.NewQuadToEnd
        );
        lastFillOutSegment[1] = lastSegFilledState;
        lastFillOutSegment[2] = lastSegFillingOutState;
        _transSegments[_transSegmentsIndexer.x++] = lastFillOutSegment;
    }

    private void BuildFillInData(in float inFillTotalDistance, ref QSTransSegment lastFillOutSegment)
    {
        float lerpOffset = _emptyZoneLength / inFillTotalDistance;
        QSTransSegFillData firstSegFillInData = new QSTransSegFillData(
            new float2(0, lerpOffset),
            QuadConstructType.ContinueQuadFromStart
        );
        lastFillOutSegment[0] = firstSegFillInData;
        lastFillOutSegment[1].SetLerpRange(new float2(lerpOffset, lastFillOutSegment[1].LerpRange.y));

        int inFillDistancesIndexer = 0;
        float2 dr = new float2(lerpOffset, _inFillDistances[inFillDistancesIndexer++] / inFillTotalDistance + lerpOffset);

        for (int i = 0; i < _target.QuadsCount; i++)
        { 
            float2x4 startEndSeg = _startEndSegs[_transSegmentsIndexer.x];
            float2x2 startLineSeg = new float2x2(startEndSeg[0], startEndSeg[1]);
            float2x2 endLineSeg = new float2x2(startEndSeg[2], startEndSeg[3]);

            if (i == 0)
            {
                QSTransSegment firstFillInSegment = new QSTransSegment(startLineSeg, endLineSeg, fillDataLength: 2);
                QSTransSegFillData fillInState = new QSTransSegFillData(
                    new float2(dr.x, dr.y),
                    QuadConstructType.NewQuadToEnd
                );
                QSTransSegFillData filledState = new QSTransSegFillData(
                    new float2(dr.y, 1),
                    QuadConstructType.NewQuadStartToEnd
                );
                firstFillInSegment[0] = fillInState;
                firstFillInSegment[1] = filledState;
                _transSegments[_transSegmentsIndexer.x++] = firstFillInSegment;
            }
            else if (i < _target.QuadsCount - 1)
            {
                QSTransSegment segment = new QSTransSegment(startLineSeg, endLineSeg, fillDataLength: 2);
                QSTransSegFillData fillInState = new QSTransSegFillData(
                    new float2(dr.x, dr.y),
                    QuadConstructType.ContinueQuadFromStart
                );
                QSTransSegFillData filledState = new QSTransSegFillData(
                    new float2(dr.y, 1),
                    QuadConstructType.NewQuadStartToEnd
                );
                segment[0] = fillInState;
                segment[1] = filledState;
                _transSegments[_transSegmentsIndexer.x++] = segment;
            }
            else
            {
                QSTransSegment lastFillInSegment = new QSTransSegment(startLineSeg, endLineSeg, fillDataLength: 1);
                lastFillInSegment[0] = new QSTransSegFillData(
                    new float2(dr.x, 1),
                    QuadConstructType.ContinueQuadFromStart
                );
                _transSegments[_transSegmentsIndexer.x] = lastFillInSegment;
                break;
            }

            dr.x = dr.y;
            if (_transSegmentsIndexer.x < _target.QuadsCount)
            { 
                dr.y += _inFillDistances[_transSegmentsIndexer.x] / inFillTotalDistance;
            }
        }
    }
}