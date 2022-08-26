using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
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

    private MeshBuffersIndexers _buffersData;

    private float3 _startRay;
    private quaternion _rotationDelta;
    private float _angleDelta;

    private float2 _currentRadii;
    private float3x2 _normalAndUV;

    public void Execute()
    {
        _buffersData = new MeshBuffersIndexers();

        _startRay = new float3(math.cos( TAU / 4), 0, math.sin( TAU / 4));
        _angleDelta = TAU / P_SideCount;
        _angleDelta = _angleDelta / P_SegmentResolution;
        _rotationDelta = quaternion.AxisAngle(math.up(), _angleDelta); 

        float radiusDelta = (P_OuterCircleRadius - P_InnerCircleRadius) / P_RingCount;

        bool isInitializedSegmentVertexPositions = false;
        float3x2 meshData = new float3x2(
            _startRay,
            float3.zero
        );

        _normalAndUV = new float3x2(
            math.up(),
            float3.zero
        );
        float deltaUV = 1.0f / P_SideCount;
        float startUV = 1 - deltaUV / 2;

        for (int side = 0; side < P_SideCount; side++)
        {
            _normalAndUV[1] = new float3(0, startUV - side * deltaUV, 0);

            _currentRadii.x = P_InnerCircleRadius;
            _currentRadii.y = _currentRadii.x + radiusDelta;

            for (int ring = 0; ring < P_RingCount; ring++)
            {
                AddSegment();

                if (!isInitializedSegmentVertexPositions)
                {
                    meshData[1].xy = _currentRadii.xy;
                    meshData[1].z = _angleDelta;

                    OutputSegmentMeshes[ring] = new WheelSegmentMesh(meshData, P_SegmentResolution);
                }
                _currentRadii.x = _currentRadii.y;
                _currentRadii.y += radiusDelta;
            }
            if (!isInitializedSegmentVertexPositions)
            {
                isInitializedSegmentVertexPositions = true;
            }
        }
    }

    private void AddSegment()
    {
        _buffersData.LocalCount = int2.zero;

        QuadStripBuilderVertexData quadStrip = 
            new QuadStripBuilderVertexData(OutputVertices, OutputIndices, _normalAndUV);

        float3 currentRay = _startRay;
        float2x2 quadStripSegment = new float2x2(
            (currentRay * _currentRadii.x).xz, 
            (currentRay * _currentRadii.y).xz);
        quadStrip.Start(quadStripSegment, ref _buffersData);

        for (int i = 0; i < P_SegmentResolution; i++)
        {
            currentRay =  math.rotate(_rotationDelta, currentRay);
            quadStripSegment[0] = (currentRay * _currentRadii.x).xz;
            quadStripSegment[1] = (currentRay * _currentRadii.y).xz;
            quadStrip.Continue(quadStripSegment, ref _buffersData);
        }
    }
}