using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using static Orazum.Math.MathUtilities;

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
    public NativeArray<WheelSegmentMesh> OutputSegmentMeshes; // one for each ring

    private short _totalVertexCount;
    private short _totalIndexCount;

    private short _segmentVertexCount;
    private short _segmentIndexCount;

    private float2 _uv;
    private float _currentRadius;
    private float _nextRadius;

    private float3 _startRay;

    private float _startAngle; 
    private float _angleResolutionDelta;

    private int2 _prevQuadStripIndices;

    public void Execute()
    {
        _startAngle = TAU / 4;
        _startRay = new float3(math.cos(_startAngle), 0, math.sin(_startAngle));
        
        _angleResolutionDelta = TAU / P_SideCount;
        _angleResolutionDelta = _angleResolutionDelta / P_SegmentResolution;

        float radiusDelta = (P_OuterCircleRadius - P_InnerCircleRadius) / P_RingCount;
        float lerpDeltaToResolution = 1 / P_SegmentResolution;
        bool isInitializedSegmentVertexPositions = false;
        float3x2 meshData = new float3x2(
            _startRay,
            float3.zero
        );

        for (int side = 0; side < P_SideCount; side++)
        {
            float deltaUV = 1.0f / P_SideCount;
            float startUV = 1 - deltaUV / 2;
            _uv = new float2(0, startUV - side * deltaUV);

            _currentRadius = P_InnerCircleRadius;
            _nextRadius = _currentRadius + radiusDelta;


            for (int ring = 0; ring < P_RingCount; ring++)
            {
                AddSegment();

                if (!isInitializedSegmentVertexPositions)
                {
                    meshData[1].x = _currentRadius;
                    meshData[1].y = _nextRadius;
                    meshData[1].z = _angleResolutionDelta;

                    OutputSegmentMeshes[ring] = new WheelSegmentMesh(meshData, P_SegmentResolution);
                } 
                _currentRadius = _nextRadius;
                _nextRadius += radiusDelta;
            }
            if (!isInitializedSegmentVertexPositions)
            {
                isInitializedSegmentVertexPositions = true;
            }
        }
    }

    private void AddSegment()
    {
        _segmentVertexCount = 0;
        _segmentIndexCount = 0;

        float3 currentRay = _startRay;
        float2x2 quadStrip = new float2x2(
            (currentRay * _currentRadius).xz, 
            (currentRay * _nextRadius).xz);
        StartQuadStrip(quadStrip);

        quaternion q = quaternion.AxisAngle(math.up(), _angleResolutionDelta);

        for (int i = 0; i < P_SegmentResolution; i++)
        {
            currentRay =  math.rotate(q, currentRay);
            quadStrip[0] = (currentRay * _currentRadius).xz;
            quadStrip[1] = (currentRay * _nextRadius).xz;
            ContinueQuadStrip(quadStrip);
        }
    }

    private void StartQuadStrip(float2x2 p)
    {
        _prevQuadStripIndices.x = AddVertex(p[0]);
        _prevQuadStripIndices.y = AddVertex(p[1]);
    }

    private void ContinueQuadStrip(float2x2 p)
    {
        int2 newQuadStripIndices = int2.zero;
        newQuadStripIndices.x = AddVertex(p[0]);
        newQuadStripIndices.y = AddVertex(p[1]);

        int4 quadIndices = new int4(_prevQuadStripIndices, newQuadStripIndices.yx);
        AddQuadIndices(quadIndices);

        _prevQuadStripIndices = newQuadStripIndices;
    }

    private void AddQuadIndices(int4 quadIndices)
    {
        AddIndex(quadIndices.x);
        AddIndex(quadIndices.y);
        AddIndex(quadIndices.z);
        AddIndex(quadIndices.x);
        AddIndex(quadIndices.z);
        AddIndex(quadIndices.w);
    }

    private short AddVertex(float2 pos)
    { 
        VertexData vertex = new VertexData();
        vertex.position = x0z(pos);
        vertex.normal = math.up();
        vertex.uv = _uv;
        
        OutputVertices[_totalVertexCount++] = vertex;
        short addedVertexIndex = _segmentVertexCount;
        _segmentVertexCount++;

        return addedVertexIndex;
    }

    private void AddIndex(int vertexIndex)
    {
        OutputIndices[_totalIndexCount++] = (short)vertexIndex;
    }
}