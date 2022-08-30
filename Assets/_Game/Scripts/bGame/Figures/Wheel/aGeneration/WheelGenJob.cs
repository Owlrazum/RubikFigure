using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
using static Orazum.Constants.Math;
using static Orazum.Math.LineSegmentUtilities;
using static QSTransSegment;

[BurstCompile]
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
    public NativeArray<QSTransSegment> OutputUpTransSegments;

    [WriteOnly]
    public NativeArray<QSTransSegment> OutputAntiCWTransSegments;

    [WriteOnly]
    public NativeArray<QSTransSegment> OutputCWTransSegments;

    private MeshBuffersIndexers _buffersData;

    private float3 _segStartRay;
    private float3x2 _normalAndUV;

    private float3 _radiiData;

    private quaternion _segQuadRotDelta;

    private NativeArray<float3> _transitionGrid;
    private int _sideOffset;
    private int _ringOffset;

    private int4 _transSegIndexer;
    private int2 _levitatinoSegIndexer;

    public void Execute()
    {
        _sideOffset = P_SegmentResolution;
        _ringOffset = _sideOffset * P_SideCount;
        int transitionGridVerticesCount = _ringOffset * (P_RingCount + 1);
        _transitionGrid = new NativeArray<float3>(transitionGridVerticesCount, Allocator.Temp);
        _transSegIndexer = int4.zero;
        _levitatinoSegIndexer = new int2(P_RingCount - 1, P_RingCount - 1);


        _buffersData = new MeshBuffersIndexers();

        _segStartRay = new float3(math.cos(TAU / 4), 0, math.sin(TAU / 4));
        _normalAndUV = new float3x2(
            math.up(),
            float3.zero
        );

        _segQuadRotDelta = quaternion.AxisAngle(math.up(),TAU / P_SideCount / P_SegmentResolution);

        _radiiData.xy = new float2(P_InnerCircleRadius, P_OuterCircleRadius);
        _radiiData.z = (P_OuterCircleRadius - P_InnerCircleRadius) / P_RingCount;

        float deltaUV = 1.0f / P_SideCount;
        float startUV = 1 - deltaUV / 2;

        for (int side = 0; side < P_SideCount; side++)
        {
            _normalAndUV[1] = new float3(0, startUV - side * deltaUV, 0);

            _radiiData.x = P_InnerCircleRadius;
            _radiiData.y = _radiiData.x + _radiiData.z;

            quaternion sideRot = quaternion.AxisAngle(math.up(), TAU / P_SideCount * side);
            _segStartRay = math.rotate(sideRot, _segStartRay);

            for (int ring = 0; ring < P_RingCount; ring++)
            {
                AddSegment(side, ring);

                _radiiData.x = _radiiData.y;
                _radiiData.y += _radiiData.z;
            }
        }

        GenerateTransitions();
    }

    private void AddSegment(int side, int ring)
    {
        _buffersData.LocalCount = int2.zero;

        QuadStripBuilderVertexData quadStrip =
            new QuadStripBuilderVertexData(OutputVertices, OutputIndices, _normalAndUV);

        float3 currentRay = _segStartRay;
        float3x2 quadStripSegment = new float3x2(
            (currentRay * _radiiData.x),
            (currentRay * _radiiData.y));
        quadStrip.Start(quadStripSegment, ref _buffersData);

        int gridIndexer = side * _sideOffset + ring * _ringOffset;
        _transitionGrid[gridIndexer++] = quadStripSegment[0];

        for (int i = 0; i < P_SegmentResolution; i++)
        {
            currentRay = math.rotate(_segQuadRotDelta, currentRay);
            quadStripSegment[0] = (currentRay * _radiiData.x);
            quadStripSegment[1] = (currentRay * _radiiData.y);
            quadStrip.Continue(quadStripSegment, ref _buffersData);

            if (i < P_SegmentResolution - 1)
            {
                _transitionGrid[gridIndexer++] = quadStripSegment[0];
            }
        }

        if (ring == P_RingCount - 1)
        {
            currentRay = _segStartRay;
            float3 outerRingVertex = currentRay * _radiiData.y;
            gridIndexer = side * _sideOffset + (ring + 1) * _ringOffset;
            _transitionGrid[gridIndexer++] = outerRingVertex;
            for (int i = 0; i < P_SegmentResolution - 1; i++)
            {
                outerRingVertex = math.rotate(_segQuadRotDelta, outerRingVertex);
                _transitionGrid[gridIndexer++] = outerRingVertex;
            }
        }
    }

    private void GenerateTransitions()
    {
        for (int side = 0; side < P_SideCount; side++)
        {
            for (int ring = 0; ring < P_RingCount; ring++)
            {
                int4 originIndexer = int4.zero;
                int4 targetIndexer = int4.zero;
                float3x4 originQuad = float3x4.zero;
                float3x4 targetQuad = float3x4.zero;

                GetSegmentIndexer(side * _sideOffset + ring * _ringOffset, out targetIndexer);
                GetTransQuad(in targetIndexer, ref targetQuad);

                int nextSide = side + 1 >= P_SideCount ? 0 : side + 1;
                GetSegmentIndexer(nextSide * _sideOffset + ring * _ringOffset, out originIndexer);
                GetTransQuad(in originIndexer, ref originQuad);
                GenerateClockOrderTransition(in originQuad, in targetQuad, isCW: true);

                int prevSide = side - 1 < 0 ? P_SideCount - 1 : side - 1;
                GetSegmentIndexer(prevSide * _sideOffset + ring * _ringOffset, out originIndexer);
                GetTransQuad(in originIndexer, ref originQuad);
                GenerateClockOrderTransition(in originQuad, in targetQuad, isCW: false);

                GetQuadIndexer(side * _sideOffset + ring * _ringOffset, out targetIndexer);
                for (int r = 0; r < P_SegmentResolution; r++)
                {
                    GetTransQuad(in targetIndexer, ref targetQuad);

                    targetIndexer.x += 1;
                    GetQuadIndexer(targetIndexer.x, out targetIndexer);

                    if (ring == P_RingCount - 1)
                    {
                        GetQuadIndexer(side * _sideOffset, out originIndexer);
                        GetTransQuad(in originIndexer, ref originQuad);
                        GenerateLevitationVerticalTransition(in originQuad, in targetQuad, isDown: true);
                    }
                    else
                    {
                        GetQuadIndexer(targetIndexer.z, out originIndexer);
                        GetTransQuad(in originIndexer, ref originQuad);
                        GenerateGroundedVerticalTransition(in originQuad, in targetQuad, isDown: true);
                    }

                    if (ring == 0)
                    {
                        GetQuadIndexer(side * _sideOffset + (P_RingCount - 1) * _ringOffset, out originIndexer);
                        GetTransQuad(in originIndexer, ref originQuad);
                        GenerateLevitationVerticalTransition(in originQuad, in targetQuad, isDown: false);
                    }
                    else
                    {
                        GetQuadIndexer(targetIndexer.x - _ringOffset, out originIndexer);
                        GetTransQuad(in originIndexer, ref originQuad);
                        GenerateGroundedVerticalTransition(in originQuad, in targetQuad, isDown: false);
                    }
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

        QSTransSegFillRadialData radialData = new QSTransSegFillRadialData();
        radialData.AxisAngles = new float4x2(
            new float4(rotAxises[0], 180),
            new float4(rotAxises[1], 180)
        );
        radialData.Centers = new float3x2()
        {
            
        };
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
            OutputDownTransSegments[_levitatinoSegIndexer.x++] = originLinear;
            OutputDownTransSegments[_levitatinoSegIndexer.x++] = radial;
            OutputDownTransSegments[_levitatinoSegIndexer.x++] = targetLinear;
        }
        else
        {
            OutputDownTransSegments[_levitatinoSegIndexer.y++] = originLinear;
            OutputDownTransSegments[_levitatinoSegIndexer.y++] = radial;
            OutputDownTransSegments[_levitatinoSegIndexer.y++] = targetLinear;
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
            OutputDownTransSegments[_transSegIndexer.x++] = origin;
            OutputDownTransSegments[_transSegIndexer.x++] = target;
        }
        else
        { 
            OutputDownTransSegments[_transSegIndexer.y++] = origin;
            OutputDownTransSegments[_transSegIndexer.y++] = target;
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
        float rotationAngle = TAU / P_SideCount;
        if (!isCW)
        {
            rotationAngle = -rotationAngle;
        }

        float4x2 axisAngles = new float4x2(
            new float4(math.up(), rotationAngle),
            new float4(math.up(), rotationAngle)
        );
        QSTransSegFillRadialData radialData = new QSTransSegFillRadialData();
        radialData.Resolution = P_SegmentResolution;
        radialData.Centers = float3x2.zero;
        radialData.AxisAngles = axisAngles;
        radialFillData.RadialData = radialData;
        radial[0] = radialFillData;

        if (isCW)
        { 
            OutputDownTransSegments[_transSegIndexer.z++] = radial;
        }
        else
        { 
            OutputDownTransSegments[_transSegIndexer.w++] = radial;
        }
    }

    private void GetQuadIndexer(int bottomLeftIndex, out int4 quadIndexer)
    {
        quadIndexer.x = bottomLeftIndex;
        quadIndexer.y = quadIndexer.x + 1;
        quadIndexer.z = quadIndexer.x + _ringOffset;
        quadIndexer.w = quadIndexer.z + 1;
    }

    // segment mesh of the wheel, one for each side and ring
    private void GetSegmentIndexer(int bottomLeftIndex, out int4 segmentIndexer)
    {
        segmentIndexer.x = bottomLeftIndex;
        segmentIndexer.y = segmentIndexer.x + P_SegmentResolution;
        segmentIndexer.z = segmentIndexer.x + _ringOffset;
        segmentIndexer.w = segmentIndexer.z + P_SegmentResolution;
    }

    private void GetLineSegments(in float3x4 quad, in int4 indexers, out float3x2 startLineSeg, out float3x2 endLineSeg)
    { 
        startLineSeg = new float3x2(quad[indexers.x], quad[indexers.y]);
        endLineSeg = new float3x2(quad[indexers.z], quad[indexers.w]);
    }

    private void GetTransQuad(in int4 transIndex, ref float3x4 transQuad)
    {
        transQuad[0] = _transitionGrid[transIndex.x];
        transQuad[1] = _transitionGrid[transIndex.y];
        transQuad[2] = _transitionGrid[transIndex.z];
        transQuad[3] = _transitionGrid[transIndex.w];
    }
}