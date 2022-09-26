using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
using static Orazum.Math.RaysUtilities;
using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Math.MathUtils;
using static Orazum.Constants.Math;

[BurstCompile]
public struct ValknutGenJob : IJob
{
    private const float ValknutRatio = 3;

    public float P_InnerTriangleRadius;
    public float P_Width;
    public float P_GapSize;

    [WriteOnly]
    public NativeArray<VertexData> OutVertices;

    [WriteOnly]
    public NativeArray<short> OutIndices;

    [WriteOnly]
    public QuadStripsBuffer OutQuadStripsCollection;

    private MeshBuffersIndexers _buffersData;
    private int2 _quadStripsCollectionIndexer;

    private float3x2 _normalAndUV;

    private float3 _startRay;
    private quaternion _rightRotate;
    private quaternion _leftRotate;

    private int _quadStripIndexer;

    public void Execute()
    {
        _startRay = new float3(0, 0, 1);
        _rightRotate = quaternion.AxisAngle(math.up(), TAU / 3);
        _leftRotate = quaternion.AxisAngle(math.up(), 2 * TAU / 3);

        _buffersData = new MeshBuffersIndexers();
        _quadStripsCollectionIndexer = int2.zero;

        float startUV = 1 - 1.0f / 6;
        _normalAndUV = new float3x2(
            math.up(),
            new float3(0, startUV, 0)
        );

        _quadStripIndexer = 0;
        GenerateValknut();
    }

    private void OffsetUV()
    {
        _normalAndUV[1].y -= 1.0f / 3;
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

        float4x3 urDir = new float4x3(dirs[2], dirs[1], dirs[0]); 

        float2x3 urPos = new float2x3();
        urPos[0] = rightTriangle.Up;
        urPos[1] = upTriangle.Left;
        urPos[2] = upTriangle.Up;
        float2x4 urEdges = ConstructTwoAngleSegment(urPos, in urDir);
        ConstructOneAngleSegment(in urEdges, in urDir, upTriangle.Right);
        OffsetUV();


        float4x3 rlDir = new float4x3(dirs[0], dirs[2], dirs[1]);

        float2x3 rlPos = new float2x3();
        rlPos[0] = leftTriangle.Right;
        rlPos[1] = rightTriangle.Up;
        rlPos[2] = rightTriangle.Right;
        float2x4 rlEdges = ConstructTwoAngleSegment(rlPos, in rlDir);
        ConstructOneAngleSegment(in rlEdges, in rlDir, rightTriangle.Left);
        OffsetUV();


        float4x3 luDir = new float4x3(dirs[1], dirs[0], dirs[2]);

        float2x3 luPos = new float2x3();
        luPos[0] = upTriangle.Left;
        luPos[1] = leftTriangle.Right;
        luPos[2] = leftTriangle.Left;
        float2x4 luEdges = ConstructTwoAngleSegment(luPos, in luDir);
        ConstructOneAngleSegment(in luEdges, in luDir, leftTriangle.Up);
        OffsetUV();
    }

    private struct TwoAngleSegment // tas
    {
        public float2x2 s1;
        public float2x2 s2;
        public float2x2 s3;
        public float2x2 s4;
    }

    private struct OneAngleSegment // oas
    {
        public float2x2 s1;
        public float2x2 s2;
        public float2x2 s3;
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
    private float2x4 ConstructTwoAngleSegment(float2x3 poses, in float4x3 dirs)
    { 
        TwoAngleSegment tas = new TwoAngleSegment();
        float2 intersect, v1, v2;

        IntersectRays2D(Ray(poses[0], dirs[0].zw), Ray(poses[1], dirs[1].zw), out intersect);
        v2 = GapVertex(intersect, dirs[1].xy);
        v1 = ExtrudeVertex(v2, dirs[0].xy);
        tas.s1 = new float2x2(v1, v2);

        IntersectRays2D(Ray(poses[1], dirs[0].xy), Ray(tas.s1[0], dirs[1].xy), out intersect);
        v2 = poses[1];
        v1 = ExtrudeVertex(intersect, dirs[1].zw);
        tas.s2 = new float2x2(v1, v2);

        IntersectRays2D(Ray(poses[2], dirs[2].xy), Ray(tas.s2[0], dirs[0].xy), out intersect);
        v2 = poses[2];
        v1 = ExtrudeVertex(intersect, dirs[0].zw);
        tas.s3 = new float2x2(v1, v2);

        IntersectRays2D(Ray(poses[0], dirs[0].zw), Ray(poses[2], dirs[2].xy), out intersect);
        v2 = GapVertex(intersect, dirs[2].zw);
        v1 = ExtrudeVertex(v2, dirs[0].zw);
        tas.s4 = new float2x2(v1, v2);

        AddTwoAngleSegmentMeshData(tas);

        float2x4 edges = new float2x4();
        edges[0] = tas.s4[0];
        edges[1] = tas.s4[1];
        edges[2] = tas.s1[0];
        edges[3] = tas.s1[1];
        return edges;
    }

    private void ConstructOneAngleSegment(in float2x4 edges, in float4x3 dirs, float2 triangleVertex)
    {
        float offsetLength = P_GapSize * 2 + P_Width;
        float2 intersect, v1, v2;
        
        OneAngleSegment oas = new OneAngleSegment();
        v1 = OffsetVertex(edges[0], dirs[2].xy, offsetLength);
        v2 = OffsetVertex(edges[1], dirs[2].xy, offsetLength);
        oas.s1 = new float2x2(v1, v2);
        
        IntersectRays2D(Ray(edges[0], dirs[2].xy), Ray(edges[2], dirs[1].zw), out intersect);
        v1 = intersect;
        v2 = triangleVertex;
        oas.s2 = new float2x2(v1, v2);

        v1 = OffsetVertex(edges[2], dirs[1].zw, offsetLength);
        v2 = OffsetVertex(edges[3], dirs[1].zw, offsetLength);
        oas.s3 = new float2x2(v1, v2);

        AddOneAngleSegment(oas);
    }

    private void AddOneAngleSegment(OneAngleSegment oas)
    {
        _buffersData.LocalCount = int2.zero;

        QuadStripBuilder quadStripBuilder = 
            new QuadStripBuilder(OutVertices, OutIndices, _normalAndUV);
        quadStripBuilder.Start(x0z(oas.s1), ref _buffersData);
        quadStripBuilder.Continue(x0z(oas.s2), ref _buffersData);
        quadStripBuilder.Continue(x0z(oas.s3), ref _buffersData);

        _quadStripsCollectionIndexer.y = 3;
        NativeArray<float3x2> lineSegments = 
            OutQuadStripsCollection.GetBufferSegmentAndWriteIndexer(_quadStripsCollectionIndexer, _quadStripIndexer++);
        lineSegments[0] = x0z(oas.s1);
        lineSegments[1] = x0z(oas.s2);
        lineSegments[2] = x0z(oas.s3);
        _quadStripsCollectionIndexer.x += 3;
    }

    private void AddTwoAngleSegmentMeshData(TwoAngleSegment tas)
    {
        _buffersData.LocalCount = int2.zero;

        QuadStripBuilder quadStripBuilder 
            = new QuadStripBuilder(OutVertices, OutIndices, _normalAndUV);
        quadStripBuilder.Start(x0z(tas.s1), ref _buffersData);
        quadStripBuilder.Continue(x0z(tas.s2), ref _buffersData);
        quadStripBuilder.Continue(x0z(tas.s3), ref _buffersData);
        quadStripBuilder.Continue(x0z(tas.s4), ref _buffersData);

        _quadStripsCollectionIndexer.y = 4;
        NativeArray<float3x2> lineSegments = 
            OutQuadStripsCollection.GetBufferSegmentAndWriteIndexer(_quadStripsCollectionIndexer, _quadStripIndexer++);
        lineSegments[0] = x0z(tas.s1);
        lineSegments[1] = x0z(tas.s2);
        lineSegments[2] = x0z(tas.s3);
        lineSegments[3] = x0z(tas.s4);
        _quadStripsCollectionIndexer.x += 4;
    }

    private float2 ExtrudeVertex(float2 start, float2 direction)
    {
        return start + direction * P_Width;
    }

    private float2 GapVertex(float2 start, float2 direction)
    { 
        return start + direction * P_GapSize;
    }

    private float2 OffsetVertex(float2 vertex, float2 direction, float length)
    {
        return vertex + direction * length;
    }
}
