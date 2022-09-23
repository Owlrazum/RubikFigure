using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using Orazum.Meshing;
using static Orazum.Math.RaysUtilities;
using static Orazum.Math.MathUtils;
using static Orazum.Meshing.QST_Segment;
using static Orazum.Meshing.QSTS_FillData;

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

    private NativeArray<QST_Segment> _writeBuffer;
    private NativeArray<float2x4> _startEndSegs;
    private NativeArray<float> _outFillDistances;
    private NativeArray<float> _inFillDistances;

    private int2 _transSegmentsIndexer;
    private float _emptyZoneLength;

    public bool IsValid;

    public ValknutTransitionsBuilder(
        in QuadStrip origin,
        in QuadStrip target
    )
    {
        _origin = origin;
        _target = target;
        _writeBuffer = new NativeArray<QST_Segment>();

        _transSegmentsIndexer = new int2(0, _origin.QuadsCount + 1 + _target.QuadsCount);

        _startEndSegs = new NativeArray<float2x4>(_transSegmentsIndexer.y, Allocator.Temp);
        _outFillDistances = new NativeArray<float>(_origin.QuadsCount, Allocator.Temp);
        _inFillDistances = new NativeArray<float>(_target.QuadsCount, Allocator.Temp);

        _emptyZoneLength = 0;
        IsValid = true;

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

    public void BuildTransition(ref NativeArray<QST_Segment> writeBuffer)
    {
        _writeBuffer = writeBuffer;
        ComputeDistancesAndSegs(
            out float2 fillInOutTotalDistances
        );
        if (!IsValid)
        {
            return;
        }
        BuildFillOutData(in fillInOutTotalDistances.y, out QST_Segment lastSegment);
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
        if (!IsValid)
        {
            return;
        }

        int4 indexer = new int4(0, 1, 0, 1);
        if (_transitionType == TransitionType.OasToOas || _transitionType == TransitionType.TasToOas)
        {
            indexer = indexer.xywz;
        }

        int inDistancesIndexer = 0;
        GetIndexTarget(_target.LineSegmentsCount, out index);
        for (int i = index.x; index.y > 0 ? i < index.z : i >= index.z; i += index.y)
        {
            float2x2 startSeg = new float2x2(_target[i][0].xz, _target[i][1].xz);
            float2x2 endSeg = new float2x2(_target[i + index.y][0].xz, _target[i + index.y][1].xz);
            _startEndSegs[_transSegmentsIndexer.x++] = 
                new float2x4(startSeg[indexer.x], startSeg[indexer.y], endSeg[indexer.z], endSeg[indexer.w]);

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
        DrawRay(r1, 1, 10);
        DrawRay(r2, 1, 10);
        DrawRay(targetRay, 10, 10);
        if (!intersect)
        {
            IsValid = false;
            UnityEngine.Debug.LogError($"{_transitionType} is not intersected");
            return;
        }
        // Assert.IsTrue(intersect, $"{_transitionType} is not intersected");

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
        out QST_Segment lastFillOutSegment
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
                QSTS_BuilderUtils.PrepareSegment(x0z(startLineSeg), x0z(endLineSeg), QSTS_Type.Quad, 
                    fillDataLength: 1, out QST_Segment firstSegment);
                QSTS_FillData fillOutState = new QSTS_FillData(
                    ConstructType.New,
                    FillType.ToEnd,
                    new float2(0, dr.y)
                );
                firstSegment[0] = fillOutState;
                _writeBuffer[_transSegmentsIndexer.x++] = firstSegment;
            }
            else
            {
                QSTS_BuilderUtils.PrepareSegment(x0z(startLineSeg), x0z(endLineSeg), QSTS_Type.Quad, 
                    fillDataLength: 2, out QST_Segment segment);
                QSTS_FillData filledState = new QSTS_FillData(
                    ConstructType.Continue,
                    FillType.StartToEnd,
                    new float2(0, dr.x)
                );

                QSTS_FillData fillingOutState = new QSTS_FillData(
                    ConstructType.New,
                    FillType.ToEnd,
                    new float2(dr.x, dr.y)
                );

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
        
        QSTS_BuilderUtils.PrepareSegment(x0z(lastStartLineSeg), x0z(lastEndLineSeg), QSTS_Type.Quad, 
                    fillDataLength: 3, out lastFillOutSegment);
        QSTS_FillData lastSegFilledState = new QSTS_FillData(
            ConstructType.Continue,
            FillType.StartToEnd,            
            new float2(0, dr.x)
        );

        QSTS_FillData lastSegFillingOutState = new QSTS_FillData(
            ConstructType.New,
            FillType.ToEnd,
            new float2(dr.x, 1)
        );

        lastFillOutSegment[1] = lastSegFilledState;
        lastFillOutSegment[2] = lastSegFillingOutState;
        _writeBuffer[_transSegmentsIndexer.x++] = lastFillOutSegment;
    }

    private void BuildFillInData(in float inFillTotalDistance, ref QST_Segment lastFillOutSegment)
    {
        float lerpOffset = _emptyZoneLength / inFillTotalDistance;
        QSTS_FillData firstSegFillInData = new QSTS_FillData(
            ConstructType.Continue,
            FillType.FromStart,
            new float2(0, lerpOffset)
        );
        lastFillOutSegment[0] = firstSegFillInData;
        QSTS_FillData lastFilledState = lastFillOutSegment[1];
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
                QSTS_BuilderUtils.PrepareSegment(x0z(startLineSeg), x0z(endLineSeg), QSTS_Type.Quad, 
                    fillDataLength: 2, out QST_Segment firstFillInSegment);

                QSTS_FillData fillInState = new QSTS_FillData(
                    ConstructType.New,
                    FillType.FromStart,
                    new float2(dr.x, dr.y)
                );

                QSTS_FillData filledState = new QSTS_FillData(
                    ConstructType.New,
                    FillType.StartToEnd,
                    new float2(dr.y, 1)
                );

                firstFillInSegment[0] = fillInState;
                firstFillInSegment[1] = filledState;

                _writeBuffer[_transSegmentsIndexer.x++] = firstFillInSegment;
            }
            else if (i < _target.QuadsCount - 1)
            {
                QSTS_BuilderUtils.PrepareSegment(x0z(startLineSeg), x0z(endLineSeg), QSTS_Type.Quad, 
                    fillDataLength: 2, out QST_Segment segment);
                QSTS_FillData fillInState = new QSTS_FillData(
                    ConstructType.Continue,
                    FillType.FromStart,
                    new float2(dr.x, dr.y)
                );
                QSTS_FillData filledState = new QSTS_FillData(
                    ConstructType.New,
                    FillType.StartToEnd,
                    new float2(dr.y, 1)
                );
                segment[0] = fillInState;
                segment[1] = filledState;
                _writeBuffer[_transSegmentsIndexer.x++] = segment;
            }
            else
            {
                QSTS_BuilderUtils.PrepareSegment(x0z(startLineSeg), x0z(endLineSeg), QSTS_Type.Quad, 
                    fillDataLength: 1, out QST_Segment lastFillInSegment);
                QSTS_FillData fillInState = new QSTS_FillData(
                    ConstructType.Continue,
                    FillType.FromStart,
                    new float2(dr.x, 1)
                );
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