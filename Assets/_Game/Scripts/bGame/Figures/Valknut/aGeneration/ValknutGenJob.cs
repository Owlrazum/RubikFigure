using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using static Orazum.Math.MathUtilities;

[BurstCompile]
public struct ValknutGenJob : IJob
{
    private const float VALKNUT_RATIO = 3;

    public float P_InnerTriangleRadius;
    public float P_Width;
    public float P_GapSize;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    [WriteOnly]
    public NativeArray<short> OutputIndices;

    [WriteOnly]
    public NativeArray<ValknutSegmentMesh> OutputSegmentMeshes;

    private short _segmentIndex;
    private short _segmentVertexCount;
    private short _totalVertexCount;
    private short _totalIndexCount;

    private float2 _uv;

    private float3 _startRay;
    private quaternion _rightRotate;
    private quaternion _leftRotate;

    private int2 _prevQuadStripIndices;

    public void Execute()
    {
        _startRay = new float3(0, 0, 1);
        _rightRotate = quaternion.AxisAngle(math.up(), TAU / 3);
        _leftRotate = quaternion.AxisAngle(math.up(), 2 * TAU / 3);

        _segmentIndex = 0;
        _segmentVertexCount = 0;
        _totalVertexCount = 0;
        _totalIndexCount = 0;

        float startUV = 1 - 1.0f / 6;
        _uv = new float2(0, startUV);

        GenerateValknut();
    }

    private void OffsetUV()
    {
        _uv.y -= 1.0f / 3;
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

        float valknutRadius = P_InnerTriangleRadius * VALKNUT_RATIO;

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
        float2x4 urEdges = ConstructTwoAngleSegment(urPos, in urDir);
        ConstructOneAngleSegment(in urEdges, in urDir, upTriangle.Right);
        OffsetUV();


        float4x3 rlDir = new float4x3(dirs[0], dirs[1], dirs[2]);

        float2x3 rlPos = new float2x3();
        rlPos[0] = leftTriangle.Right;
        rlPos[1] = rightTriangle.Right;
        rlPos[2] = rightTriangle.Up;
        float2x4 rlEdges = ConstructTwoAngleSegment(rlPos, in rlDir);
        ConstructOneAngleSegment(in rlEdges, in rlDir, rightTriangle.Left);
        OffsetUV();


        float4x3 luDir = new float4x3(dirs[1], dirs[2], dirs[0]);

        float2x3 luPos = new float2x3();
        luPos[0] = upTriangle.Left;
        luPos[1] = leftTriangle.Left;
        luPos[2] = leftTriangle.Right;
        float2x4 luEdges = ConstructTwoAngleSegment(luPos, in luDir);
        ConstructOneAngleSegment(in luEdges, in luDir, leftTriangle.Up);
        OffsetUV();
    }

    private void SwapDirs(int2 swaps, ref float4x3 dirs)
    {
        float4 t = dirs[swaps.x];
        dirs[swaps.x] = dirs[swaps.y];
        dirs[swaps.y] = t;
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

    // TODO: swap dirs so the first one will be for left quad.

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

        IntersectRays(Ray(poses[0], dirs[0].zw), Ray(poses[1], dirs[1].xy), out intersect);
        v1 = GapVertex(intersect, dirs[1].zw);
        v2 = ExtrudeVertex(v1, dirs[0].zw);
        tas.s1 = new float2x2(v1, v2);

        IntersectRays(Ray(poses[1], dirs[0].zw), Ray(tas.s1[1], dirs[1].zw), out intersect);
        v1 = poses[1];
        v2 = ExtrudeVertex(intersect, dirs[1].xy);
        tas.s2 = new float2x2(v1, v2);

        IntersectRays(Ray(tas.s2[1], dirs[0].zw), Ray(poses[2], dirs[2].zw), out intersect);
        v1 = poses[2];
        v2 = ExtrudeVertex(intersect, dirs[0].xy);
        tas.s3 = new float2x2(v1, v2);

        IntersectRays(Ray(poses[0], dirs[0].zw), Ray(poses[2], dirs[2].zw), out intersect);
        v1 = GapVertex(intersect, dirs[2].xy);
        v2 = ExtrudeVertex(v1, dirs[0].xy);
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
        v1 = OffsetVertex(edges[0], dirs[2].zw, offsetLength);
        v2 = OffsetVertex(edges[1], dirs[2].zw, offsetLength);
        oas.s1 = new float2x2(v1, v2);
        
        IntersectRays(Ray(edges[1], dirs[2].zw), Ray(edges[3], dirs[1].xy), out intersect);
        v1 = triangleVertex;
        v2 = intersect;
        oas.s2 = new float2x2(v1, v2);

        v1 = OffsetVertex(edges[2], dirs[1].xy, offsetLength);
        v2 = OffsetVertex(edges[3], dirs[1].xy, offsetLength);
        oas.s3 = new float2x2(v1, v2);

        AddOneAngleSegment(oas);
    }

    private struct TwoAngleSegment
    {
        public float2x2 s1;
        public float2x2 s2;
        public float2x2 s3;
        public float2x2 s4;
    }

    private struct OneAngleSegment
    {
        public float2x2 s1;
        public float2x2 s2;
        public float2x2 s3;
    }

    
    private void AddOneAngleSegment(OneAngleSegment oneAngleSegment)
    {
        _segmentVertexCount = 0;
        StartQuadStrip(oneAngleSegment.s1);
        ContinueQuadStrip(oneAngleSegment.s2);
        ContinueQuadStrip(oneAngleSegment.s3);

        float4x4 stripsData = new float4x4(
            new float4(oneAngleSegment.s1[0], oneAngleSegment.s1[1]),
            new float4(oneAngleSegment.s2[0], oneAngleSegment.s2[1]),
            new float4(oneAngleSegment.s3[0], oneAngleSegment.s3[1]),
            float4.zero
        );
        OutputSegmentMeshes[_segmentIndex++] = new ValknutSegmentMesh(in stripsData, stripSegmentsCount: 3);
    }

    private void AddTwoAngleSegmentMeshData(TwoAngleSegment twoAngleSegment)
    {
        _segmentVertexCount = 0;
        StartQuadStrip(twoAngleSegment.s1);
        ContinueQuadStrip(twoAngleSegment.s2);
        ContinueQuadStrip(twoAngleSegment.s3);
        ContinueQuadStrip(twoAngleSegment.s4);

        float4x4 stripsData = new float4x4(
            new float4(twoAngleSegment.s1[0], twoAngleSegment.s1[1]),
            new float4(twoAngleSegment.s2[0], twoAngleSegment.s2[1]),
            new float4(twoAngleSegment.s3[0], twoAngleSegment.s3[1]),
            new float4(twoAngleSegment.s4[0], twoAngleSegment.s4[1])
        );
        OutputSegmentMeshes[_segmentIndex++] = new ValknutSegmentMesh(in stripsData, stripSegmentsCount: 4);
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