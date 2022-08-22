using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
using static Orazum.Math.MathUtilities;

[BurstCompile]
public struct ValknutSegmentPointGenJob : IJob
{
    private const float ValknutRatio = 3;

    public float P_Height;
    public float P_InnerTriangleRadius;
    public float P_Width;
    public float P_GapSize;

    [WriteOnly]
    public NativeArray<float3> OutputCollidersVertices;

    [WriteOnly]
    public NativeArray<short> OutputCollidersIndices;

    [WriteOnly]
    public NativeArray<float3> OutputRenderVertices;

    [WriteOnly]
    public NativeArray<short> OutputRenderIndices;

    private MeshBuffersData _rendererBuffersData;
    private MeshBuffersData _colliderBuffersData;

    private int _segmentIndex;

    private float3 _heightOffset;

    private float3 _startRay;
    private quaternion _rightRotate;
    private quaternion _leftRotate;

    public void Execute()
    {
        _rendererBuffersData = new MeshBuffersData();
        _colliderBuffersData = new MeshBuffersData();
        _segmentIndex = 0;

        _heightOffset = new float3(0, P_Height, 0);

        _startRay = new float3(0, 0, 1);
        _rightRotate = quaternion.AxisAngle(math.up(), TAU / 3);
        _leftRotate = quaternion.AxisAngle(math.up(), 2 * TAU / 3);

        GenerateValknut();
    }

    private struct Triangle
    {
        public float2 Up;
        public float2 Right;
        public float2 Left;

        public void Rotate(quaternion rotation)
        {
            Up = math.rotate(rotation, x0z(Up)).xz;
            Right = math.rotate(rotation, x0z(Right)).xz;
            Left = math.rotate(rotation, x0z(Left)).xz;
        }

        public void Offset(float2 offset)
        {
            Up += offset;
            Right += offset;
            Left += offset;
        }
    }

    private Triangle MakeTriangle(float3 triangleVertex)
    {
        Triangle triangle = new Triangle()
        {
            Up = triangleVertex.xz,
            Right = math.rotate(_rightRotate, triangleVertex).xz,
            Left = math.rotate(_leftRotate, triangleVertex).xz
        };

        return triangle;
    }

    private float4 Ray(float2 pos, float2 dir)
    {
        return new float4(pos, dir);
    }

    private float4x3 CalculateDirs(Triangle triangle)
    { 
        float4x3 raysCW = new float4x3();
        raysCW[0] = new float4(math.normalize(triangle.Right - triangle.Up),    math.normalize(triangle.Up    - triangle.Right));
        raysCW[1] = new float4(math.normalize(triangle.Left  - triangle.Right), math.normalize(triangle.Right - triangle.Left));
        raysCW[2] = new float4(math.normalize(triangle.Up    - triangle.Left),  math.normalize(triangle.Left  - triangle.Up));
        return raysCW;
    }
    

    private void GenerateValknut()
    {
        Triangle centerTriangle = MakeTriangle(new float3(0, 0, P_InnerTriangleRadius));
        centerTriangle.Rotate(quaternion.AxisAngle(math.up(), TAU / 4));

        float valknutRadius = P_InnerTriangleRadius * ValknutRatio;

        Triangle upTriangle = MakeTriangle(new float3(0, 0, valknutRadius));
        upTriangle.Offset(centerTriangle.Left);

        Triangle rightTriangle = MakeTriangle(new float3(0, 0, valknutRadius));
        rightTriangle.Offset(centerTriangle.Up);

        Triangle leftTriangle = MakeTriangle(new float3(0, 0, valknutRadius));
        leftTriangle.Offset(centerTriangle.Right);

        float4x3 dirs  = CalculateDirs(upTriangle);

        float4x3 urDir = new float4x3(dirs[2], dirs[0], dirs[1]); 

        float2x3 urPos = new float2x3();
        urPos[0] = rightTriangle.Up;
        urPos[1] = upTriangle.Up;
        urPos[2] = upTriangle.Left;
        float3x4 urEdges = ConstructTwoAngleSegment(urPos, in urDir, out TwoAngleSegment urTas);
        ConstructOneAngleSegment(in urEdges, in urDir, upTriangle.Right, out OneAngleSegment urOas);


        float4x3 rlDir = new float4x3(dirs[0], dirs[1], dirs[2]);

        float2x3 rlPos = new float2x3();
        rlPos[0] = leftTriangle.Right;
        rlPos[1] = rightTriangle.Right;
        rlPos[2] = rightTriangle.Up;
        float3x4 rlEdges = ConstructTwoAngleSegment(rlPos, in rlDir, out TwoAngleSegment rlTas);
        ConstructOneAngleSegment(in rlEdges, in rlDir, rightTriangle.Left, out OneAngleSegment rlOas);


        float4x3 luDir = new float4x3(dirs[1], dirs[2], dirs[0]);

        float2x3 luPos = new float2x3();
        luPos[0] = upTriangle.Left;
        luPos[1] = leftTriangle.Left;
        luPos[2] = leftTriangle.Right;
        float3x4 luEdges = ConstructTwoAngleSegment(luPos, in luDir, out TwoAngleSegment luTas);
        ConstructOneAngleSegment(in luEdges, in luDir, leftTriangle.Up, out OneAngleSegment luOas);
    }

    private float3 ExtrudeVertex(float3 start, float3 direction)
    {
        return start + direction * P_Width;
    }

    private float3 GapVertex(float3 start, float3 direction)
    { 
        return start + direction * P_GapSize;
    }

    private float3 OffsetVertex(float3 vertex, float3 direction, float length)
    {
        return vertex + direction * length;
    }

    /// <summary>
    /// poses[0]: vertex of triangle from which intersection
    /// poses[1]: triangleVertex for s2
    /// poses[2]: trianglevertex for s3;
    /// dirs[0] is cutting direction, dirs[1] for s1, s2, dirs[2] for s3, s4
    /// </summary>
    /// <returns>
    /// edges with edges[0] and edges[2] on inner lines of one angle segment;
    /// edges[1] and edges[3] are on outer
    /// </returns>
    private float3x4 ConstructTwoAngleSegment(float2x3 poses, in float4x3 dirs, out TwoAngleSegment tas)
    { 
        tas = new TwoAngleSegment();
        float3 intersect, v1, v2, v1h, v2h;

        IntersectRays(Ray(poses[0], dirs[0].zw), Ray(poses[1], dirs[1].xy), out intersect);
        v1 = GapVertex(intersect, x0z(dirs[1].zw));
        v2 = ExtrudeVertex(v1, x0z(dirs[0].zw));
        v2h = ExtrudeVertex(v2, _heightOffset);
        v1h = ExtrudeVertex(v1, _heightOffset);
        tas.s1 = new float3x4(v1, v2, v2h, v1h);

        IntersectRays(Ray(poses[1], dirs[0].zw), Ray(tas.s1[1].xz, dirs[1].zw), out intersect);
        v1 = x0z(poses[1]);
        v2 = ExtrudeVertex(intersect, x0z(dirs[1].xy));
        v2h = ExtrudeVertex(v2, _heightOffset);
        v1h = ExtrudeVertex(v1, _heightOffset);
        tas.s2 = new float3x4(v1, v2, v2h, v1h);

        IntersectRays(Ray(tas.s2[1].xz, dirs[0].zw), Ray(poses[2], dirs[2].zw), out intersect);
        v1 = x0z(poses[2]);
        v2 = ExtrudeVertex(intersect, x0z(dirs[0].xy));
        v2h = ExtrudeVertex(v2, _heightOffset);
        v1h = ExtrudeVertex(v1, _heightOffset);
        tas.s3 = new float3x4(v1, v2, v2h, v1h);

        IntersectRays(Ray(poses[0], dirs[0].zw), Ray(poses[2], dirs[2].zw), out intersect);
        v1 = GapVertex(intersect, x0z(dirs[2].xy));
        v2 = ExtrudeVertex(v1, x0z(dirs[0].xy));
        v2h = ExtrudeVertex(v2, _heightOffset);
        v1h = ExtrudeVertex(v1, _heightOffset);
        tas.s4 = new float3x4(v1, v2, v2h, v1h);

        AddTwoAngleSegmentRendererMesh(tas);
        AddTwoAngleSegmentColliderMesh(tas);

        float3x4 edges = new float3x4();
        edges[0] = tas.s4[0];
        edges[1] = tas.s4[1];
        edges[2] = tas.s1[0];
        edges[3] = tas.s1[1];
        return edges;
    }

    private void ConstructOneAngleSegment(in float3x4 edges, in float4x3 dirs, float2 triangleVertex, out OneAngleSegment oas)
    {
        float offsetLength = P_GapSize * 2 + P_Width;
        float3 intersect, v1, v2, v1h, v2h;
        
        oas = new OneAngleSegment();
        v1 = OffsetVertex(edges[0], x0z(dirs[2].zw), offsetLength);
        v2 = OffsetVertex(edges[1], x0z(dirs[2].zw), offsetLength);
        v2h = ExtrudeVertex(v2, _heightOffset);
        v1h = ExtrudeVertex(v1, _heightOffset);
        oas.s1 = new float3x4(v1, v2, v2h, v1h);
        
        IntersectRays(Ray(edges[1].xz, dirs[2].zw), Ray(edges[3].xz, dirs[1].xy), out intersect);
        v1 = x0z(triangleVertex);
        v2 = intersect;
        v2h = ExtrudeVertex(v2, _heightOffset);
        v1h = ExtrudeVertex(v1, _heightOffset);
        oas.s2 = new float3x4(v1, v2, v2h, v1h);

        v1 = OffsetVertex(edges[2], x0z(dirs[1].xy), offsetLength);
        v2 = OffsetVertex(edges[3], x0z(dirs[1].xy), offsetLength);
        v2h = ExtrudeVertex(v2, _heightOffset);
        v1h = ExtrudeVertex(v1, _heightOffset);
        oas.s3 = new float3x4(v1, v2, v2h, v1h);

        AddOneAngleSegmentRendererMesh(oas);
        AddOneAngleSegmentColliderMesh(oas);
    }

    private struct TwoAngleSegment
    {
        public float3x4 s1;
        public float3x4 s2;
        public float3x4 s3;
        public float3x4 s4;
    }

    private struct OneAngleSegment
    {
        public float3x4 s1;
        public float3x4 s2;
        public float3x4 s3;
    }
    
    private void AddOneAngleSegmentColliderMesh(OneAngleSegment oneAngleSegment)
    {
        CubeStripSegmented cubeStrip = new CubeStripSegmented(OutputCollidersVertices, OutputCollidersIndices);
        cubeStrip.Start(oneAngleSegment.s1, ref _colliderBuffersData);
        cubeStrip.Continue(oneAngleSegment.s2, ref _colliderBuffersData);
        cubeStrip.Finish(oneAngleSegment.s3, ref _colliderBuffersData);
    }

    private void AddTwoAngleSegmentColliderMesh(TwoAngleSegment twoAngleSegment)
    {
        CubeStripSegmented cubeStrip = new CubeStripSegmented(OutputCollidersVertices, OutputCollidersIndices);
        cubeStrip.Start(twoAngleSegment.s1, ref _colliderBuffersData);
        cubeStrip.Continue(twoAngleSegment.s2, ref _colliderBuffersData);
        cubeStrip.Continue(twoAngleSegment.s3, ref _colliderBuffersData);
        cubeStrip.Finish(twoAngleSegment.s4, ref _colliderBuffersData);
    }

    private void AddOneAngleSegmentRendererMesh(OneAngleSegment oneAngleSegment)
    {
        _rendererBuffersData.LocalCount = int2.zero; 
        CubeStrip cubeStrip = new CubeStrip(OutputRenderVertices, OutputRenderIndices);
        cubeStrip.Start(oneAngleSegment.s1, ref _rendererBuffersData);
        cubeStrip.Continue(oneAngleSegment.s2, ref _rendererBuffersData);
        cubeStrip.Finish(oneAngleSegment.s3, ref _rendererBuffersData);
    }

    private void AddTwoAngleSegmentRendererMesh(TwoAngleSegment twoAngleSegment)
    {
        _rendererBuffersData.LocalCount = int2.zero;
        CubeStrip cubeStrip = new CubeStrip(OutputRenderVertices, OutputRenderIndices);
        cubeStrip.Start(twoAngleSegment.s1, ref _rendererBuffersData);
        cubeStrip.Continue(twoAngleSegment.s2, ref _rendererBuffersData);
        cubeStrip.Continue(twoAngleSegment.s3, ref _rendererBuffersData);
        cubeStrip.Finish(twoAngleSegment.s4, ref _rendererBuffersData);
    }
}