using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Collections;
using static Orazum.Collections.IndexUtilities;

using Orazum.Math;

using static Orazum.Meshing.BufferUtils;

// OT - origin target
// QSTS - quad strip transition segment
public class WheelGeneratorTransitions : FigureGeneratorTransitions
{
    private const int LevitationTransSegCount = 3;
    private const int ClockOrderTransSegCount = 1;
    private const int VerticalTransSegCountPerQuad = 2;

    private int VerticalTransSegCount; // const like
    private int SideTransSegCount;
    private int RingTransSegCount;

    private int2 _sidesRingsCount;
    private int _segmentResolution;
    private int4 _transSegsCount;

    public override void StartGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    {
        _sidesRingsCount = quadStripsCollection.Dims;
        _segmentResolution = quadStripsCollection.GetQuadIndexer(0).y;

        int sideTransitions = _sidesRingsCount.x * 2;
        int ringTransitions = _sidesRingsCount.y * 2;
        int totalTransitionsCount = sideTransitions + ringTransitions;

        VerticalTransSegCount = VerticalTransSegCountPerQuad * _segmentResolution;
        SideTransSegCount = VerticalTransSegCount * _sidesRingsCount.y - 2 + LevitationTransSegCount * 2;
        RingTransSegCount = ClockOrderTransSegCount * _sidesRingsCount.x;


        _transSegsCount.x = VerticalTransSegCount * 2 * (_sidesRingsCount.y - 1) * _sidesRingsCount.x;
        _transSegsCount.y = LevitationTransSegCount * 2 * _sidesRingsCount.x;
        _transSegsCount.z = ClockOrderTransSegCount * 2 * _sidesRingsCount.x * _sidesRingsCount.y;
        _transSegsCount.w = _transSegsCount.x + _transSegsCount.y + _transSegsCount.z;

        NativeArray<int2> OT_bufferIndexers =
            new NativeArray<int2>(totalTransitionsCount, Allocator.TempJob);
        int OT_sideTransitions = sideTransitions * _sidesRingsCount.y;
        int OT_ringTransitions = ringTransitions * _sidesRingsCount.x;

        NativeArray<int2> OT_buffer =
            new NativeArray<int2>(OT_sideTransitions + OT_ringTransitions, Allocator.TempJob);
        SegmentedBufferInt2 OT =
            new SegmentedBufferInt2(OT_buffer, OT_bufferIndexers);

        NativeArray<int2> QSTS_bufferIndexers = new NativeArray<int2>(totalTransitionsCount, Allocator.Persistent);
        GenerateDataJobIndexData(ref OT, ref QSTS_bufferIndexers);

        NativeArray<QSTransSegment> QSTS_buffer = new NativeArray<QSTransSegment>(_transSegsCount.w, Allocator.Persistent);
        _transitionsCollection = new QSTransitionsBuffer(QSTS_buffer, QSTS_bufferIndexers);

        WheelGenJobTransData transitionDataJob = new WheelGenJobTransData()
        {
            InQuadStripsCollection = quadStripsCollection,
            InOriginsTargetsIndices = OT,
            OutTransitionsCollection = _transitionsCollection
        };
        _dataJobHandle = transitionDataJob.ScheduleParallel(totalTransitionsCount, 32, dependency);
    }

    private void GenerateDataJobIndexData(
        ref SegmentedBufferInt2 OT,
        ref NativeArray<int2> QSTS_buffersIndexers)
    {
        int QSTS_indexer = 0;
        int2 QSTS_bufferIndexer = new int2(0, SideTransSegCount);

        int OT_indexer = 0;
        int2 OT_bufferIndexer = new int2(0, _sidesRingsCount.y);
        for (int side = 0; side < _sidesRingsCount.x; side++)
        {
            QSTS_buffersIndexers[QSTS_indexer++] = QSTS_bufferIndexer;
            MoveBufferIndexer(ref QSTS_bufferIndexer, SideTransSegCount);

            var upBuffer = OT.GetBufferSegmentAndWriteIndexer(OT_bufferIndexer, OT_indexer++);
            MoveBufferIndexer(ref OT_bufferIndexer, _sidesRingsCount.y);
            var downBuffer = OT.GetBufferSegmentAndWriteIndexer(OT_bufferIndexer, OT_indexer++);
            MoveBufferIndexer(ref OT_bufferIndexer, _sidesRingsCount.y);

            int prevRing = _sidesRingsCount.y - 1;
            int ring = 0;
            int nextRing = 1;

            int bottomIndex = GetIndex(side, prevRing);
            int middleIndex = GetIndex(side, ring);
            int upperIndex = GetIndex(side, nextRing);

            while (ring < _sidesRingsCount.y)
            {
                downBuffer[ring] = new int2(upperIndex, middleIndex);
                upBuffer[ring] = new int2(bottomIndex, middleIndex);
                IncreaseRing(ref prevRing);
                IncreaseRing(ref ring);
                IncreaseRing(ref nextRing);
            }
        }

        for (int ring = 0; ring < _sidesRingsCount.y; ring++)
        {
            QSTS_buffersIndexers[QSTS_indexer++] = QSTS_bufferIndexer;
            MoveBufferIndexer(ref QSTS_bufferIndexer, RingTransSegCount);
        }

        // for (int ring = 0; ring < _sidesRingsCount.y; ring++)
        // {
        //     int2 sideRingIndex = new int2(side, ring);
        //     int targetIndex = XyToIndex(sideRingIndex, _sidesRingsCount.y);

        //     int2x4 originsIndices = new int2x4(
        //         Figure.MoveIndexClockOrder(sideRingIndex, ClockOrderType.CW, _sidesRingsCount),
        //         Figure.MoveIndexClockOrder(sideRingIndex, ClockOrderType.AntiCW, _sidesRingsCount),
        //         Figure.MoveIndexVertOrder(sideRingIndex, VertOrderType.Up, _sidesRingsCount),
        //         Figure.MoveIndexVertOrder(sideRingIndex, VertOrderType.Down, _sidesRingsCount)
        //     );

        //     bool4 areOutOfDims = new bool4(

        //         Figure.IsOutOfDimsClockOrder(sideRingIndex, ClockOrderType.CW, _sidesRingsCount),
        //         Figure.IsOutOfDimsClockOrder(sideRingIndex, ClockOrderType.AntiCW, _sidesRingsCount),
        //         Figure.IsOutOfDimsVertOrder(sideRingIndex, VertOrderType.Up, _sidesRingsCount),
        //         Figure.IsOutOfDimsVertOrder(sideRingIndex, VertOrderType.Down, _sidesRingsCount)
        //     );

        //     for (int i = 0; i < 4; i++)
        //     {
        //         int originIndex = XyToIndex(originsIndices[i], _sidesRingsCount.y);
        //         originTargetIndices[originTargetIndexer++] = new int2(originIndex, targetIndex);

        //     }
        // }
        // int2 originIndices = new int2(4, 5);

        // int bufferStart = 0;
        // int2 rangesCount = new int2(7, 6);

        // int originTargetIndicesIndexer = 0;
        // int buffersIndexersIndexer = 0;
        // for (int i = 0; i < 6; i += 2)
        // {
        //     originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.x, targetIndex);
        //     QSTS_buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.x);
        //     bufferStart += rangesCount.x;

        //     originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.y, targetIndex);
        //     QSTS_buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.y);
        //     bufferStart += rangesCount.y;

        //     originIndices.x = originIndices.x + 2 >= 6 ? 0 : originIndices.x + 2;
        //     originIndices.y = originIndices.y + 2 >= 6 ? 1 : originIndices.y + 2;
        //     targetIndex += 2;
        // }

        // targetIndex = 1;
        // originIndices = new int2(5, 4);
        // rangesCount = new int2(5, 6);
        // for (int i = 0; i < 6; i += 2)
        // {
        //     originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.x, targetIndex);
        //     QSTS_buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.x);
        //     bufferStart += rangesCount.x;

        //     originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.y, targetIndex);
        //     QSTS_buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.y);
        //     bufferStart += rangesCount.y;

        //     originIndices.x = originIndices.x + 2 >= 6 ? 1 : originIndices.x + 2;
        //     originIndices.y = originIndices.y + 2 >= 6 ? 0 : originIndices.y + 2;
        //     targetIndex += 2;
        // }
    }

    private int GetIndex(int side, int ring)
    {
        return XyToIndex(side, ring, _sidesRingsCount.y);
    }

    private void IncreaseRing(ref int ring)
    {
        if (ring + 1 >= _sidesRingsCount.y)
        {
            ring = 0;
        }
        else
        {
            ring++;
        }
    }

    public override void FinishGeneration(Figure figure)
    {
        // Array2D<WheelSegmentTransitions> transitionDatas = new Array2D<WheelSegmentTransitions>(_sidesRingsCount);
        // int2x3 index = int2x3.zero;
        // for (int side = 0; side < _sidesRingsCount.x; side++)
        // {
        //     for (int ring = 0; ring < _sidesRingsCount.y; ring++)
        //     {
        //         NativeArray<QSTransSegment> atsi = _atsi.GetSubArray(index[0].x, ClockOrderTransSegCount);
        //         NativeArray<QSTransSegment> ctsi = _ctsi.GetSubArray(index[0].x, ClockOrderTransSegCount);
        //         index[0].x += ClockOrderTransSegCount;

        //         NativeArray<QSTransSegment> dtsi;
        //         if (ring == 0)
        //         {
        //             dtsi = _levDtsi.GetSubArray(index[1].x, LevitationTransSegCount);
        //             index[1].x += LevitationTransSegCount;
        //         }
        //         else
        //         {
        //             dtsi = _dtsi.GetSubArray(index[2].x, VerticalTransSegCountPerQuad * _segmentResolution);
        //             index[2].x += VerticalTransSegCountPerQuad * _segmentResolution;
        //         }

        //         NativeArray<QSTransSegment> utsi;
        //         if (ring == _sidesRingsCount.y - 1)
        //         {
        //             utsi = _levUtsi.GetSubArray(index[1].y, LevitationTransSegCount);
        //             index[1].y += LevitationTransSegCount;
        //         }
        //         else
        //         {
        //             utsi = _utsi.GetSubArray(index[2].y, VerticalTransSegCountPerQuad * _segmentResolution);
        //             index[2].y += VerticalTransSegCountPerQuad * _segmentResolution;
        //         }

        //         WheelSegmentTransitions transData = new WheelSegmentTransitions();
        //         WheelSegmentTransitions.Atsi(ref transData) = new QSTransition(atsi);
        //         WheelSegmentTransitions.Ctsi(ref transData) = new QSTransition(ctsi);
        //         WheelSegmentTransitions.Dtsi(ref transData) = new QSTransition(dtsi);
        //         WheelSegmentTransitions.Utsi(ref transData) = new QSTransition(utsi);

        //         transitionDatas[side, ring] = transData;
        //     }
        // }
        // _wheel.AssignTransitionDatas(transitionDatas);
        // throw new System.NotImplementedException();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // _originsTargetsIndices.DisposeIfNeeded();
    }
}
/*
_transitionGrid = new NativeArray<float3>(
        _sidesRingsCount.x * (_sidesRingsCount.y + 1) * _segmentResolution, Allocator.TempJob);

    _dtsi = new NativeArray<QSTransSegment>(_transSegsCount.x, Allocator.Persistent);
    _utsi = new NativeArray<QSTransSegment>(_transSegsCount.x, Allocator.Persistent);

    _levDtsi = new NativeArray<QSTransSegment>(_transSegsCount.y, Allocator.Persistent);
    _levUtsi = new NativeArray<QSTransSegment>(_transSegsCount.y, Allocator.Persistent);

    _ctsi = new NativeArray<QSTransSegment>(_transSegsCount.z, Allocator.Persistent);
    _atsi = new NativeArray<QSTransSegment>(_transSegsCount.z, Allocator.Persistent);
*/