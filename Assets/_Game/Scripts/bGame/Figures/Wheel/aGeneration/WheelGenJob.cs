using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using Orazum.Meshing;
using static Orazum.Constants.Math;
using static Orazum.Math.LineSegmentUtilities;
using static QSTransSegment;
using static Orazum.Utilities.DebugUtilities;

// [BurstCompile]
public struct WheelGenJob : IJob
{
    public int P_SideCount;
    public int P_RingCount;
    public int P_SegmentResolution;

    public float P_InnerCircleRadius;
    public float P_OuterCircleRadius;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    [WriteOnly]
    public NativeArray<short> OutputIndices;

    [WriteOnly]
    public NativeArray<QSTransSegment> OutputDownTransSegments;
    [WriteOnly]
    public NativeArray<QSTransSegment> OutputLevDownTransSegments;

    [WriteOnly]
    public NativeArray<QSTransSegment> OutputUpTransSegments;
    [WriteOnly]
    public NativeArray<QSTransSegment> OutputLevUpTransSegments;

    [WriteOnly]
    public NativeArray<QSTransSegment> OutputAntiCWTransSegments;
    [WriteOnly]
    public NativeArray<QSTransSegment> OutputCWTransSegments;

    public NativeArray<float3> _HelperTransitionGrid;

    private MeshBuffersIndexers _buffersData;

    private float3 _segStartRay;
    private float3 _sideStartRay;
    private float3x2 _normalAndUV;

    private float3 _radiiData;

    private quaternion _segQuadRotDelta;

    private int2 _dutsi;
    private int2 _levDutsi;
    private int2 _actsi;

    public void Execute()
    {
        _dutsi = int2.zero;
        _levDutsi = int2.zero;
        _actsi = int2.zero;


        _buffersData = new MeshBuffersIndexers();

        _segStartRay = new float3(math.cos(TAU / 4), 0, math.sin(TAU / 4));
        _normalAndUV = new float3x2(
            math.up(),
            float3.zero
        );

        _segQuadRotDelta = quaternion.AxisAngle(math.up(), TAU / P_SideCount / P_SegmentResolution);

        _radiiData.xy = new float2(P_InnerCircleRadius, P_OuterCircleRadius);
        _radiiData.z = (P_OuterCircleRadius - P_InnerCircleRadius) / P_RingCount;

        float deltaUV = 1.0f / P_SideCount;
        float startUV = 1 - deltaUV / 2;

        int2 offsets = new int2(P_SegmentResolution, P_SegmentResolution * P_SideCount);

        for (int side = 0; side < P_SideCount; side++)
        {
            _normalAndUV[1] = new float3(0, startUV - side * deltaUV, 0);

            _radiiData.x = P_InnerCircleRadius;
            _radiiData.y = _radiiData.x + _radiiData.z;

            quaternion sideRot = quaternion.AxisAngle(math.up(), TAU / P_SideCount * side);
            _sideStartRay = math.rotate(sideRot, _segStartRay);

            for (int ring = 0; ring < P_RingCount; ring++)
            {
                AddSegment(side, ring, offsets);

                _radiiData.x = _radiiData.y;
                _radiiData.y += _radiiData.z;
            }
        }

        // DebugHelperGrid(offsets);

        GenerateTransitions(offsets);
    }

    private void AddSegment(int side, int ring, int2 offsets)
    {
        _buffersData.LocalCount = int2.zero;

        QuadStripBuilderVertexData quadStripBuilder =
            new QuadStripBuilderVertexData(OutputVertices, OutputIndices, _normalAndUV);

        float3 currentRay = _sideStartRay;
        float3x2 quadStripSegment = new float3x2(
            (currentRay * _radiiData.x),
            (currentRay * _radiiData.y));
        quadStripBuilder.Start(quadStripSegment, ref _buffersData);

        int gridIndexer = side * offsets.x + ring * offsets.y;
        for (int i = 0; i < P_SegmentResolution; i++)
        {
            _HelperTransitionGrid[gridIndexer++] = quadStripSegment[0];

            currentRay = math.rotate(_segQuadRotDelta, currentRay);
            quadStripSegment[0] = (currentRay * _radiiData.x);
            quadStripSegment[1] = (currentRay * _radiiData.y);
            quadStripBuilder.Continue(quadStripSegment, ref _buffersData);
        }

        if (ring == P_RingCount - 1)
        {
            currentRay = _sideStartRay;
            float3 outerRingVertex = currentRay * _radiiData.y;
            gridIndexer = side * offsets.x + (ring + 1) * offsets.y;
            for (int i = 0; i < P_SegmentResolution; i++)
            {
                _HelperTransitionGrid[gridIndexer++] = outerRingVertex;
                outerRingVertex = math.rotate(_segQuadRotDelta, outerRingVertex);
            }
        }
    }

    private void GenerateTransitions(int2 offsets)
    {
        for (int side = 0; side < P_SideCount; side++)
        {
            for (int ring = 0; ring < P_RingCount; ring++)
            {
                int4 originIndexer = int4.zero;
                int4 targetIndexer = int4.zero;
                float3x4 originQuad = float3x4.zero;
                float3x4 targetSegmentQuad = float3x4.zero;

                int startSegmentIndex = side * offsets.x + ring * offsets.y;
                GetSegmentIndexerGrounded(startSegmentIndex, offsets, out targetIndexer);
                GetTransQuad(in targetIndexer, ref targetSegmentQuad);

                int nextSide = side + 1 >= P_SideCount ? 0 : side + 1;
                int nextSideStartSegmentIndex = nextSide * offsets.x + ring * offsets.y;
                GetSegmentIndexerGrounded(nextSideStartSegmentIndex, offsets, out originIndexer);
                GetTransQuad(in originIndexer, ref originQuad);
                GenerateClockOrderTransition(in originQuad, in targetSegmentQuad, isCW: true);

                int prevSide = side - 1 < 0 ? P_SideCount - 1 : side - 1;
                int prevSideStartSegmentIndex = prevSide * offsets.x + ring * offsets.y;
                GetSegmentIndexerGrounded(prevSideStartSegmentIndex, offsets, out originIndexer);
                GetTransQuad(in originIndexer, ref originQuad);
                GenerateClockOrderTransition(in originQuad, in targetSegmentQuad, isCW: false);

                if (ring == 0)
                {
                    GetSegmentIndexerLevitation(startSegmentIndex, offsets, out targetIndexer);
                    GetTransQuad(in targetIndexer, ref targetSegmentQuad);
                    GetSegmentIndexerLevitation(startSegmentIndex, offsets, out originIndexer);
                    GetTransQuad(in originIndexer, ref originQuad);
                    GenerateLevitationVerticalTransition(in originQuad, in targetSegmentQuad, isDown: true);
                }

                if (ring == P_RingCount - 1)
                {
                    GetSegmentIndexerLevitation(startSegmentIndex, offsets, out targetIndexer);
                    GetTransQuad(in targetIndexer, ref targetSegmentQuad);
                    GetSegmentIndexerLevitation(startSegmentIndex, offsets, out originIndexer);
                    GetTransQuad(in originIndexer, ref originQuad);
                    GenerateLevitationVerticalTransition(in originQuad, in targetSegmentQuad, isDown: false);
                }

                int startQuadIndex = startSegmentIndex;
                float3x4 targetQuad = float3x4.zero;
                int4 targetQuadIndexer = int4.zero;
                for (int r = 0; r < P_SegmentResolution; r++)
                {
                    GetQuadIndexer(startQuadIndex, offsets, out targetQuadIndexer);
                    GetTransQuad(in targetQuadIndexer, ref targetQuad);
                    if (ring < P_RingCount - 1)
                    {
                        GetQuadIndexer(targetQuadIndexer.z, offsets, out originIndexer);
                        GetTransQuad(in originIndexer, ref originQuad);
                        GenerateGroundedVerticalTransition(in originQuad, in targetSegmentQuad, isDown: true);
                    }

                    if (ring > 0)
                    {
                        GetQuadIndexer(targetQuadIndexer.x - offsets.y, offsets, out originIndexer);
                        GetTransQuad(in originIndexer, ref originQuad);
                        GenerateGroundedVerticalTransition(in originQuad, in targetSegmentQuad, isDown: false);
                    }

                    startQuadIndex++;
                }
            }
        }
    }

    private void GenerateLevitationVerticalTransition(in float3x4 originQuad, in float3x4 targetQuad, bool isDown)
    {
        float4 distances = float4.zero;
        distances.x = DistanceLineSegment(originQuad[0], originQuad[2]);
        distances.y = DistanceLineSegment(originQuad[0], originQuad[2]) * TAU / 2;
        distances.z = DistanceLineSegment(targetQuad[0], targetQuad[2]);
        distances.w = distances.x + distances.y + distances.z;

        float2x3 lerpRanges = float2x3.zero;
        lerpRanges[0] = new float2(0, distances.x / distances.w);
        lerpRanges[1] = new float2(lerpRanges[0].y, (distances.x + distances.y) / distances.w);
        lerpRanges[2] = new float2(lerpRanges[1].y, 1);



        int4 indexers = isDown ? new int4(2, 3, 0, 1) : new int4(0, 1, 2, 3);
        float3x2 startLineSeg, endLineSeg;
        GetLineSegments(in originQuad, in indexers, out startLineSeg, out endLineSeg);
        QSTransSegment originLinear = new QSTransSegment(startLineSeg, endLineSeg, 1);
        QSTransSegFillData linearFillOut = new QSTransSegFillData(lerpRanges[0], MeshConstructType.Quad);
        linearFillOut.QuadType = QuadConstructType.NewQuadToEnd;
        originLinear[0] = linearFillOut;



        float3x4 radialQuad = float3x4.zero;
        if (isDown)
        {
            radialQuad[0] = originQuad[0];
            radialQuad[1] = originQuad[1];
            radialQuad[2] = targetQuad[2];
            radialQuad[3] = targetQuad[3];
        }
        else
        {
            radialQuad[0] = originQuad[2];
            radialQuad[1] = originQuad[3];
            radialQuad[2] = targetQuad[0];
            radialQuad[3] = targetQuad[1];
        }
        GetLineSegments(in radialQuad, new int4(0, 1, 2, 3), out startLineSeg, out endLineSeg);
        QSTransSegment radial = new QSTransSegment(startLineSeg, endLineSeg, 1);
        QSTransSegFillData radialFillData = new QSTransSegFillData(lerpRanges[1], MeshConstructType.Radial);
        radialFillData.RadialType = RadialConstructType.Double;
        
        QSTransSegFillRadialData radialData = new QSTransSegFillRadialData();

        radialData.LerpLength = _radiiData.z / (P_RingCount * _radiiData.z / 2 * TAU / 2);

        radialData.Centers = new float3x2(
            GetLineSegmentCenter(startLineSeg[0], endLineSeg[0]),
            GetLineSegmentCenter(startLineSeg[1], endLineSeg[1])
        );

        quaternion perp;
        if (isDown)
        {
            perp = quaternion.AxisAngle(math.up(), 90);
        }
        else
        {
            perp = quaternion.AxisAngle(math.up(), -90);
        }
        float3x2 rotAxises = float3x2.zero;
        rotAxises[0] = GetPerpDirection(perp, new float3x2(startLineSeg[0], endLineSeg[0]));
        rotAxises[1] = GetPerpDirection(perp, new float3x2(startLineSeg[1], endLineSeg[1]));

        radialData.Resolution = P_SegmentResolution;
        radialData.AxisAngles = new float4x2(
            new float4(rotAxises[0], 180),
            new float4(rotAxises[1], 180)
        );

        radialFillData.RadialData = radialData;
        radial[0] = radialFillData;


        indexers = isDown ? new int4(0, 1, 2, 3) : new int4(2, 3, 0, 1);
        GetLineSegments(in targetQuad, in indexers, out startLineSeg, out endLineSeg);
        QSTransSegment targetLinear = new QSTransSegment(startLineSeg, endLineSeg, 1);
        QSTransSegFillData linearFillIn = new QSTransSegFillData(lerpRanges[2], MeshConstructType.Quad);
        linearFillIn.QuadType = QuadConstructType.NewQuadToEnd;
        targetLinear[0] = linearFillIn;

        if (isDown)
        {
            OutputLevDownTransSegments[_levDutsi.x++] = originLinear;
            OutputLevDownTransSegments[_levDutsi.x++] = radial;
            OutputLevDownTransSegments[_levDutsi.x++] = targetLinear;
        }
        else
        {
            OutputLevUpTransSegments[_levDutsi.y++] = originLinear;
            OutputLevUpTransSegments[_levDutsi.y++] = radial;
            OutputLevUpTransSegments[_levDutsi.y++] = targetLinear;
        }
    }

    private void GenerateGroundedVerticalTransition(in float3x4 originQuad, in float3x4 targetQuad, bool isDown)
    {
        int4 indexers = isDown ? new int4(2, 3, 0, 1) : new int4(0, 1, 2, 3);

        float3x2 startLineSeg, endLineSeg;
        GetLineSegments(in originQuad, in indexers, out startLineSeg, out endLineSeg);
        QSTransSegment origin = new QSTransSegment(startLineSeg, endLineSeg, 1);

        QSTransSegFillData fillOutState = new QSTransSegFillData(new float2(0, 1), MeshConstructType.Quad);
        fillOutState.QuadType = QuadConstructType.NewQuadToEnd;
        origin[0] = fillOutState;

        GetLineSegments(in targetQuad, in indexers, out startLineSeg, out endLineSeg);
        QSTransSegment target = new QSTransSegment(startLineSeg, endLineSeg, 1);

        QSTransSegFillData fillInState = new QSTransSegFillData(new float2(0, 1), MeshConstructType.Quad);
        fillInState.QuadType = QuadConstructType.ContinueQuadFromStart;
        target[0] = fillInState;

        if (isDown)
        {
            OutputDownTransSegments[_dutsi.x++] = origin;
            OutputDownTransSegments[_dutsi.x++] = target;
        }
        else
        {
            OutputUpTransSegments[_dutsi.y++] = origin;
            OutputUpTransSegments[_dutsi.y++] = target;
        }
    }

    private void GenerateClockOrderTransition(in float3x4 originQuad, in float3x4 targetQuad, bool isCW)
    {
        int4 indexers = isCW ? new int4(0, 1, 2, 3) : new int4(2, 3, 0, 1);
        float3x4 radialQuad = new float3x4(
            originQuad[0], originQuad[1],
            targetQuad[2], targetQuad[3]
        );

        float3x2 startLineSeg, endLineSeg;
        GetLineSegments(in radialQuad, in indexers, out startLineSeg, out endLineSeg);
        QSTransSegment radial = new QSTransSegment(startLineSeg, endLineSeg, 1);

        QSTransSegFillData radialFillData = new QSTransSegFillData(new float2(0, 1), MeshConstructType.Radial);
        radialFillData.RadialType = RadialConstructType.Single;
        QSTransSegFillRadialData radialData = new QSTransSegFillRadialData();
        radialData.LerpLength = 1;
        radialData.StartLerpOffset = 0;

        radialData.Centers = float3x2.zero;

        float rotationAngle = TAU / P_SideCount;
        if (!isCW)
        {
            rotationAngle = -rotationAngle;
        }

        float4x2 axisAngles = new float4x2(
            new float4(math.up(), rotationAngle),
            float4.zero
        );
        radialData.Resolution = P_SegmentResolution;
        radialData.AxisAngles = axisAngles;

        radialFillData.RadialData = radialData;
        radial[0] = radialFillData;

        if (isCW)
        {
            OutputCWTransSegments[_actsi.x++] = radial;
        }
        else
        {
            OutputAntiCWTransSegments[_actsi.y++] = radial;
        }
    }

    private void GetQuadIndexer(int bottomLeftIndex, int2 offsets, out int4 quadIndexer)
    {
        int ringIndex = bottomLeftIndex / offsets.y;
        quadIndexer.x = bottomLeftIndex;
        quadIndexer.y = quadIndexer.x + 1;
        if (quadIndexer.y / offsets.y != ringIndex)
        {
            quadIndexer.y = offsets.y * ringIndex;
        }

        ringIndex++;
        quadIndexer.z = quadIndexer.x + offsets.y;
        quadIndexer.w = quadIndexer.z + 1;
        if (quadIndexer.w / offsets.y != ringIndex)
        {
            quadIndexer.w = offsets.y * ringIndex;
        }
    }

    // segment mesh of the wheel, one for each side and ring
    private void GetSegmentIndexerLevitation(int bottomLeftIndex, int2 offsets, out int4 segmentIndexer)
    {
        int ringIndex = bottomLeftIndex / offsets.y;
        segmentIndexer.x = bottomLeftIndex;
        segmentIndexer.y = segmentIndexer.x + P_SegmentResolution;
        if ((segmentIndexer.y / offsets.y) != ringIndex)
        {
            segmentIndexer.y = offsets.y * ringIndex;
        }

        segmentIndexer.z = segmentIndexer.x + offsets.y;
        segmentIndexer.w = segmentIndexer.y + offsets.y;
    }

    private void GetSegmentIndexerGrounded(int bottomLeftIndex, int2 offsets, out int4 segmentIndexer)
    {
        int ringIndex = bottomLeftIndex / offsets.y;
        segmentIndexer.x = bottomLeftIndex;
        segmentIndexer.y = segmentIndexer.x + offsets.y;
        segmentIndexer.z = segmentIndexer.x + P_SegmentResolution;
        if ((segmentIndexer.z / offsets.y) != ringIndex)
        {
            segmentIndexer.z = offsets.y * ringIndex;
        }
        segmentIndexer.w = segmentIndexer.z + offsets.y;
    }

    private void GetTransQuad(in int4 transIndex, ref float3x4 transQuad)
    {
        transQuad[0] = _HelperTransitionGrid[transIndex.x];
        transQuad[1] = _HelperTransitionGrid[transIndex.y];
        transQuad[2] = _HelperTransitionGrid[transIndex.z];
        transQuad[3] = _HelperTransitionGrid[transIndex.w];
    }

    private void GetLineSegments(in float3x4 quad, in int4 indexers, out float3x2 startLineSeg, out float3x2 endLineSeg)
    {
        startLineSeg = new float3x2(quad[indexers.x], quad[indexers.y]);
        endLineSeg = new float3x2(quad[indexers.z], quad[indexers.w]);
    }

    private void DebugHelperGrid(int2 offsets)
    {
        for (int ring = 0; ring < P_RingCount + 1; ring++)
        {
            for (int i = 0; i < offsets.y; i++)
            {
                int index = i + ring * offsets.y;
                if (index + 1 < _HelperTransitionGrid.Length)
                {
                    Debug.DrawLine(_HelperTransitionGrid[index], _HelperTransitionGrid[index + 1], Color.red, 100);
                }
            }
        }
    }
}