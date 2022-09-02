using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using static Orazum.Math.RaysUtilities;
using static Orazum.Math.MathUtils;
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

    private NativeArray<QSTransSegment> _writeBuffer;
    private NativeArray<float2x4> _startEndSegs;
    private NativeArray<float> _outFillDistances;
    private NativeArray<float> _inFillDistances;

    private int2 _transSegmentsIndexer;
    private float _emptyZoneLength;

    public ValknutTransitionsBuilder(
        in QuadStrip origin,
        in QuadStrip target
    )
    {
        _origin = origin;
        _target = target;
        _writeBuffer = new NativeArray<QSTransSegment>();

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

    public void BuildTransition(ref NativeArray<QSTransSegment> writeBuffer)
    {
        _writeBuffer = writeBuffer;
        ComputeDistancesAndSegs(
            out float2 fillInOutTotalDistances
        );
        BuildFillOutData(in fillInOutTotalDistances.y, out QSTransSegment lastSegment);
        BuildFillInData(in fillInOutTotalDistances.x, ref lastSegment);
    }

    private void ComputeDistancesAndSegs(
        out float2 fillInOutTotalDistances
    )
    {
        fillInOutTotalDistances = float2.zero;

        int outDistancesIndexer = 0;

        int3 index = int3.zero;
        GetIndexOrigin(_origin.LineSegmentsCount, out index);
        for (int i = index.x; index.y > 0 ? i < index.z : i >= index.z; i += index.y)
        {
            float2x2 startSeg = new float2x2(_origin[i][0].xz, _origin[i][1].xz);
            float2x2 endSeg = new float2x2(_origin[i + index.y][0].xz, _origin[i + index.y][1].xz);
            _startEndSegs[_transSegmentsIndexer.x++] = new float2x4(startSeg[0], startSeg[1], endSeg[0], endSeg[1]);

            float3x2 delta = _origin[i + index.y] - _origin[i];
            _outFillDistances[outDistancesIndexer] = math.length(delta[0]);
            fillInOutTotalDistances.y += _outFillDistances[outDistancesIndexer];
            outDistancesIndexer++;
        }

        GetIndexEmptyZone(_origin.LineSegmentsCount, out int emptyIndex);
        ComputeDistsPosInEmptyZone(in emptyIndex, ref fillInOutTotalDistances);

        int inDistancesIndexer = 0;
        GetIndexTarget(_target.LineSegmentsCount, out index);
        for (int i = index.x; index.y > 0 ? i < index.z : i >= index.z; i += index.y)
        {
            float2x2 startSeg = new float2x2(_target[i][0].xz, _target[i][1].xz);
            float2x2 endSeg = new float2x2(_target[i + index.y][0].xz, _target[i + index.y][1].xz);
            _startEndSegs[_transSegmentsIndexer.x++] = new float2x4(startSeg[0], startSeg[1], endSeg[0], endSeg[1]);

            float3x2 delta = _target[i + index.y] - _target[i];
            _inFillDistances[inDistancesIndexer] = math.length(delta[0]);
            fillInOutTotalDistances.x += _inFillDistances[inDistancesIndexer];
            inDistancesIndexer++;
        }

        _transSegmentsIndexer.x = 0;
    }
    private void ComputeDistsPosInEmptyZone(in int emptyIndex, ref float2 fillInOutTotalDistances)
    {
        GetIntersectionRays(out float3x4 originSegmentRays, out float3x2 targetRay);
        float3x2 r1 = new float3x2(originSegmentRays[0], originSegmentRays[1]);
        float3x2 r2 = new float3x2(originSegmentRays[2], originSegmentRays[3]);
        bool intersect = IntersectSegmentToRay2D(r1, r2, targetRay, out float3x2 intersectSegment);
        Assert.IsTrue(intersect, $"{_transitionType} is not intersected");

        float2x2 startSeg = new float2x2(_origin[emptyIndex][0].xz, _origin[emptyIndex][1].xz);
        float2x2 endSeg = new float2x2(intersectSegment[0].xz, intersectSegment[1].xz);
        _startEndSegs[_transSegmentsIndexer.x++] = new float2x4(startSeg[0], startSeg[1], endSeg[0], endSeg[1]);

        float3x2 delta = intersectSegment - _origin[emptyIndex];
        _emptyZoneLength = math.length(delta[0]);
        fillInOutTotalDistances.x += _emptyZoneLength;
        fillInOutTotalDistances.y += _emptyZoneLength;
    }
    private void GetIntersectionRays(out float3x4 originRays, out float3x2 targetRay)
    {
        switch (_transitionType)
        {
            case TransitionType.TasToTas:
                originRays = _origin.GetRays(LineEndType.End, LineEndDirectionType.StartToEnd);
                targetRay = _target.GetRay(LineEndType.Start, LineEndType.End, LineEndDirectionType.EndToStart);
                return;
            case TransitionType.TasToOas:
                originRays = _origin.GetRays(LineEndType.Start, LineEndDirectionType.EndToStart);
                targetRay = _target.GetRay(LineEndType.End, LineEndType.End, LineEndDirectionType.StartToEnd);
                return;
            case TransitionType.OasToTas:
                originRays = _origin.GetRays(LineEndType.Start, LineEndDirectionType.EndToStart);
                targetRay = _target.GetRay(LineEndType.Start, LineEndType.Start, LineEndDirectionType.EndToStart);
                return;
            case TransitionType.OasToOas:
                originRays = _origin.GetRays(LineEndType.End, LineEndDirectionType.StartToEnd);
                targetRay = _target.GetRay(LineEndType.End, LineEndType.Start, LineEndDirectionType.StartToEnd);
                return;
        }

        throw new System.Exception();
    }
    private void GetIndexOrigin(int upperBound, out int3 index)
    {
        if (_transitionType == TransitionType.TasToTas || _transitionType == TransitionType.OasToOas)
        {
            index.x = 0;
            index.y = 1;
            index.z = upperBound - 1;
        }
        else
        {
            index.x = upperBound - 1;
            index.y = -1;
            index.z = 1;
        }
    }
    private void GetIndexEmptyZone(int upperBound, out int index)
    {
        if (_transitionType == TransitionType.TasToTas || _transitionType == TransitionType.OasToOas)
        {
            index = upperBound - 1;
        }
        else
        {
            index = 0;
        }
    }
    private void GetIndexTarget(int upperBound, out int3 index)
    {
        if (_transitionType == TransitionType.TasToTas || _transitionType == TransitionType.OasToTas)
        {
            index.x = 0;
            index.y = 1;
            index.z = upperBound - 1;
        }
        else
        {
            index.x = upperBound - 1;
            index.y = -1;
            index.z = 1;
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
                QSTransSegment firstSegment = new QSTransSegment(x0z(startLineSeg), x0z(endLineSeg), fillDataLength: 1);
                QSTransSegFillData fillOutState = new QSTransSegFillData(
                    new float2(0, dr.y),
                    MeshConstructType.Quad
                );
                fillOutState.QuadType = QuadConstructType.NewQuadToEnd;
                firstSegment[0] = fillOutState;
                _writeBuffer[_transSegmentsIndexer.x++] = firstSegment;
            }
            else
            {
                QSTransSegment segment = new QSTransSegment(x0z(startLineSeg), x0z(endLineSeg), fillDataLength: 2);
                QSTransSegFillData filledState = new QSTransSegFillData(
                    new float2(0, dr.x),
                    MeshConstructType.Quad
                );
                filledState.QuadType = QuadConstructType.ContinueQuadStartToEnd;

                QSTransSegFillData fillingOutState = new QSTransSegFillData(
                    new float2(dr.x, dr.y),
                    MeshConstructType.Quad
                );
                fillingOutState.QuadType = QuadConstructType.NewQuadToEnd;

                segment[0] = filledState;
                segment[1] = fillingOutState;
                _writeBuffer[_transSegmentsIndexer.x++] = segment;
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
        lastFillOutSegment = new QSTransSegment(x0z(lastStartLineSeg), x0z(lastEndLineSeg), 3);
        QSTransSegFillData lastSegFilledState = new QSTransSegFillData(
            new float2(0, dr.x),
            MeshConstructType.Quad
        );
        lastSegFilledState.QuadType = QuadConstructType.ContinueQuadStartToEnd;

        QSTransSegFillData lastSegFillingOutState = new QSTransSegFillData(
            new float2(dr.x, 1),
            MeshConstructType.Quad
        );
        lastSegFillingOutState.QuadType = QuadConstructType.NewQuadToEnd;

        lastFillOutSegment[1] = lastSegFilledState;
        lastFillOutSegment[2] = lastSegFillingOutState;
        _writeBuffer[_transSegmentsIndexer.x++] = lastFillOutSegment;
    }

    private void BuildFillInData(in float inFillTotalDistance, ref QSTransSegment lastFillOutSegment)
    {
        float lerpOffset = _emptyZoneLength / inFillTotalDistance;
        QSTransSegFillData firstSegFillInData = new QSTransSegFillData(
            new float2(0, lerpOffset),
            MeshConstructType.Quad
        );
        firstSegFillInData.QuadType = QuadConstructType.ContinueQuadFromStart;
        lastFillOutSegment[0] = firstSegFillInData;
        QSTransSegFillData lastFilledState = lastFillOutSegment[1];
        lastFilledState.LerpRange = new float2(lerpOffset, lastFillOutSegment[1].LerpRange.y);
        lastFillOutSegment[1] = lastFilledState;
        _writeBuffer[_transSegmentsIndexer.x - 1] = lastFillOutSegment;

        int inFillDistancesIndexer = 0;
        float2 dr = new float2(lerpOffset, _inFillDistances[inFillDistancesIndexer++] / inFillTotalDistance + lerpOffset);

        for (int i = 0; i < _target.QuadsCount; i++)
        {
            float2x4 startEndSeg = _startEndSegs[_transSegmentsIndexer.x];
            float2x2 startLineSeg = new float2x2(startEndSeg[0], startEndSeg[1]);
            float2x2 endLineSeg = new float2x2(startEndSeg[2], startEndSeg[3]);

            if (i == 0)
            {
                QSTransSegment firstFillInSegment = new QSTransSegment(x0z(startLineSeg), x0z(endLineSeg), fillDataLength: 2);
                QSTransSegFillData fillInState = new QSTransSegFillData(
                    new float2(dr.x, dr.y),
                    MeshConstructType.Quad
                    
                );
                fillInState.QuadType = QuadConstructType.NewQuadFromStart;

                QSTransSegFillData filledState = new QSTransSegFillData(
                    new float2(dr.y, 1),
                    MeshConstructType.Quad
                    
                );
                filledState.QuadType = QuadConstructType.NewQuadStartToEnd;

                firstFillInSegment[0] = fillInState;
                firstFillInSegment[1] = filledState;

                _writeBuffer[_transSegmentsIndexer.x++] = firstFillInSegment;
            }
            else if (i < _target.QuadsCount - 1)
            {
                QSTransSegment segment = new QSTransSegment(x0z(startLineSeg), x0z(endLineSeg), fillDataLength: 2);
                QSTransSegFillData fillInState = new QSTransSegFillData(
                    new float2(dr.x, dr.y),
                    MeshConstructType.Quad
                    
                );
                fillInState.QuadType = QuadConstructType.ContinueQuadFromStart;
                QSTransSegFillData filledState = new QSTransSegFillData(
                    new float2(dr.y, 1),
                    MeshConstructType.Quad
                    
                );
                filledState.QuadType = QuadConstructType.NewQuadStartToEnd;
                segment[0] = fillInState;
                segment[1] = filledState;
                _writeBuffer[_transSegmentsIndexer.x++] = segment;
            }
            else
            {
                QSTransSegment lastFillInSegment = new QSTransSegment(x0z(startLineSeg), x0z(endLineSeg), fillDataLength: 1);
                QSTransSegFillData fillInState = new QSTransSegFillData(
                    new float2(dr.x, 1),
                    MeshConstructType.Quad
                );
                fillInState.QuadType = QuadConstructType.ContinueQuadFromStart;
                lastFillInSegment[0] = fillInState;
                _writeBuffer[_transSegmentsIndexer.x] = lastFillInSegment;
                break;
            }

            dr.x = dr.y;
            if (inFillDistancesIndexer < _target.QuadsCount)
            {
                dr.y += _inFillDistances[inFillDistancesIndexer++] / inFillTotalDistance;
            }
        }
    }
}