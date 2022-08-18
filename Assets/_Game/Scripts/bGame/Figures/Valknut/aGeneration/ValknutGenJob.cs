using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using static Orazum.Math.MathUtilities;

[BurstCompile]
public struct ValknutGenJob : IJob
{
    private const float VALKNUT_RATIO = 3; //3.31508424658f;

    public float P_InnerTriangleRadius;
    public float P_Width;
    public float P_GapSize;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    [WriteOnly]
    public NativeArray<short> OutputIndices;

    [WriteOnly]
    public NativeArray<ValknutSegmentMesh> OutputSegmentMeshes;

    private short _totalVertexCount;
    private short _totalIndexCount;

    private short _segmentVertexCount;

    private float2 _uv;

    private float3 _startRay;
    private quaternion _rightRotate;
    private quaternion _leftRotate;

    public void Execute()
    {
        _startRay = new float3(0, 0, 1);
        _rightRotate = quaternion.AxisAngle(math.up(), TAU / 3);
        _leftRotate = quaternion.AxisAngle(math.up(), 2 * TAU / 3);

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

    private void DrawTriangle(in Triangle triangle)
    { 
        Debug.DrawLine(x0z(triangle.Up),    x0z(triangle.Right), Color.white, 100);
        Debug.DrawLine(x0z(triangle.Right), x0z(triangle.Left), Color.white, 100);
        Debug.DrawLine(x0z(triangle.Left),  x0z(triangle.Up), Color.white, 100);
    }

    private void DrawDirs(in float4x3 dirs, in Triangle triangle)
    {
        float length = 2;
        float d1 = 0.05f, d2 = 0.05f; 
        Debug.DrawRay(x0z(triangle.Up + new float2(d1, d1))   , x0z(dirs[0].xy) * length, Color.red, 100);
        Debug.DrawRay(x0z(triangle.Right + new float2(d2, d2)), x0z(dirs[0].zw) * length, Color.magenta, 100);

        Debug.DrawRay(x0z(triangle.Right + new float2(0, -d1)), x0z(dirs[1].xy) * length, Color.red, 100);
        Debug.DrawRay(x0z(triangle.Left  + new float2(0, -d2)), x0z(dirs[1].zw) * length, Color.magenta, 100);

        Debug.DrawRay(x0z(triangle.Left + new float2(-d1, d1)), x0z(dirs[2].xy) * length, Color.red, 100);
        Debug.DrawRay(x0z(triangle.Up   + new float2(-d2, d2)), x0z(dirs[2].zw) * length, Color.magenta, 100);
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
    

    /// <summary>
    /// Perhaps direction vectors should be normalized
    /// </summary>
    private void GenerateValknut()
    {
        Triangle centerTriangle = MakeTriangle(new float3(0, 0, P_InnerTriangleRadius));
        centerTriangle.Rotate(quaternion.AxisAngle(math.up(), TAU / 4));

        // DrawTriangle(centerTriangle);

        float valknutRadius = P_InnerTriangleRadius * VALKNUT_RATIO;

        Triangle upTriangle = MakeTriangle(new float3(0, 0, valknutRadius));
        upTriangle.Offset(centerTriangle.Left);

        Triangle rightTriangle = MakeTriangle(new float3(0, 0, valknutRadius));
        rightTriangle.Offset(centerTriangle.Up);

        Triangle leftTriangle = MakeTriangle(new float3(0, 0, valknutRadius));
        leftTriangle.Offset(centerTriangle.Right);

        
        // DrawTriangle(in upTriangle);
        // DrawTriangle(in rightTriangle);
        // DrawTriangle(in leftTriangle);        

        float4x3 dirs  = CalculateDirs(upTriangle);

        float4x3 urDir = new float4x3(dirs);
        SwapDirs(new int2(0, 2), ref urDir);

        float2x3 urPos = new float2x3();
        urPos[0] = rightTriangle.Up;
        urPos[1] = upTriangle.Left;
        urPos[2] = upTriangle.Up;
        float3x4 urEdges = ConstructTwoAngleSegment(urPos, in urDir);
        ConstructOneAngleSegment(in urEdges, in urDir, x0z(upTriangle.Right));
        OffsetUV();


        float4x3 rlDir = new float4x3(dirs);
        SwapDirs(new int2(1, 2), ref rlDir);

        float2x3 rlPos = new float2x3();
        rlPos[0] = leftTriangle.Right;
        rlPos[1] = rightTriangle.Up;
        rlPos[2] = rightTriangle.Right;
        float3x4 rlEdges = ConstructTwoAngleSegment(rlPos, in rlDir);
        ConstructOneAngleSegment(in rlEdges, in rlDir, x0z(rightTriangle.Left));
        OffsetUV();


        float4x3 luDir = new float4x3(dirs);
        SwapDirs(new int2(0, 1), ref luDir);

        float2x3 luPos = new float2x3();
        luPos[0] = upTriangle.Left;
        luPos[1] = leftTriangle.Right;
        luPos[2] = leftTriangle.Left;
        float3x4 luEdges = ConstructTwoAngleSegment(luPos, in luDir);
        ConstructOneAngleSegment(in luEdges, in luDir, x0z(leftTriangle.Up));
        OffsetUV();
    }

    private void SwapDirs(int2 swaps, ref float4x3 dirs)
    {
        float4 t = dirs[swaps.x];
        dirs[swaps.x] = dirs[swaps.y];
        dirs[swaps.y] = t;
    }

   /// <summary>
   /// poses[0]: vertex of triangle from which intersection
   /// poses[1]: triangleVertex for leftQuad
   /// poses[2]: trianglevertex for rightQuad;
   /// dirs[0] is cutting direction, others were placed by trial-error
   /// </summary>
   /// <returns>
   /// edges with edges[0] and edges[2] on inner lines of one angle segment;
   /// edges[1] and edges[3] are on outer
   /// </returns>
    private float3x4 ConstructTwoAngleSegment(float2x3 poses, in float4x3 dirs)
    { 
        TwoAngleSegment tas = new TwoAngleSegment();
        float3 intersect;

        float4 triangleIntersectRay = Ray(poses[0], dirs[0].zw);

        tas.LeftQuad = new float3x4();
        IntersectRays(triangleIntersectRay, Ray(poses[1], dirs[1].zw), out intersect);
        tas.LeftQuad[3] = intersect + x0z(dirs[1].xy) * P_GapSize;
        tas.LeftQuad[2] = tas.LeftQuad[3] + x0z(dirs[0].xy) * P_Width;
        IntersectRays(Ray(poses[1], dirs[0].xy), Ray(tas.LeftQuad[2].xz, dirs[1].xy), out intersect);
        tas.LeftQuad[1] = intersect + x0z(dirs[1].zw)* P_Width;
        tas.LeftQuad[0] = x0z(poses[1]);

        tas.RightQuad = new float3x4();
        IntersectRays(triangleIntersectRay, Ray(poses[2], dirs[2].xy), out intersect);
        tas.RightQuad[2] = intersect + x0z(dirs[2].zw) * P_GapSize;
        tas.RightQuad[3] = tas.RightQuad[2] + x0z(dirs[0].zw) * P_Width;
        IntersectRays(Ray(poses[2], dirs[0].zw), Ray(tas.RightQuad[3].xz, dirs[2].zw), out intersect);
        tas.RightQuad[0] = intersect + x0z(dirs[2].xy) * P_Width;
        tas.RightQuad[1] = x0z(poses[2]);

        tas.CenterQuad = new float3x4();
        tas.CenterQuad[0] = tas.LeftQuad[1];
        tas.CenterQuad[1] = tas.LeftQuad[0];
        tas.CenterQuad[2] = tas.RightQuad[1];
        tas.CenterQuad[3] = tas.RightQuad[0];

        AddTwoAngleSegmentMeshData(tas);

        float3x4 edges = new float3x4();
        edges[0] = tas.LeftQuad[2];
        edges[1] = tas.LeftQuad[3];
        edges[2] = tas.RightQuad[3];
        edges[3] = tas.RightQuad[2];
        return edges;
    }

    private void ConstructOneAngleSegment(in float3x4 edges, in float4x3 dirs, float3 triangleVertex)
    {
        float3 intersectPos = float3.zero;
        IntersectRays(Ray(edges[2].xz, dirs[2].xy), Ray(edges[0].xz, dirs[1].zw), out intersectPos);
        
        OneAngleSegment oas = new OneAngleSegment();
        oas.LeftQuad = new float3x4();
        oas.LeftQuad[0] = triangleVertex;
        oas.LeftQuad[1] = intersectPos;
        oas.LeftQuad[2] = edges[2] + x0z(dirs[2].xy * (P_GapSize * 2 + P_Width));
        oas.LeftQuad[3] = edges[3] + x0z(dirs[2].xy * (P_GapSize * 2 + P_Width));

        oas.RightQuad = new float3x4();
        oas.RightQuad[0] = triangleVertex;
        oas.RightQuad[1] = intersectPos;
        oas.RightQuad[2] = edges[0] + x0z(dirs[1].zw * (P_GapSize * 2 + P_Width));
        oas.RightQuad[3] = edges[1] + x0z(dirs[1].zw * (P_GapSize * 2 + P_Width));

        AddOneAngleSegment(oas);
    }

    private struct TwoAngleSegment
    {
        public float3x4 LeftQuad;
        public float3x4 CenterQuad;
        public float3x4 RightQuad;
    }

    private struct OneAngleSegment
    {
        public float3x4 LeftQuad;
        public float3x4 RightQuad;
    }

    
    private void AddOneAngleSegment(OneAngleSegment oneAngleSegment)
    {
        _segmentVertexCount = 0;
        AddQuad(oneAngleSegment.LeftQuad);
        AddQuad(oneAngleSegment.RightQuad);
    }

    private void AddTwoAngleSegmentMeshData(TwoAngleSegment twoAngleSegment)
    {
        _segmentVertexCount = 0;
        // Debug.DrawRay((twoAngleSegment.LeftQuad[0] + twoAngleSegment.LeftQuad[2]) / 2, Vector3.up, Color.red, 100);
        AddQuad(twoAngleSegment.LeftQuad);
        AddQuad(twoAngleSegment.CenterQuad);
        // Debug.DrawRay((twoAngleSegment.CenterQuad[0] + twoAngleSegment.CenterQuad[2]) / 2, Vector3.up * 2, Color.red, 100);
        AddQuad(twoAngleSegment.RightQuad);
        // Debug.DrawRay((twoAngleSegment.RightQuad[0] + twoAngleSegment.RightQuad[2]) / 2, Vector3.up * 3, Color.red, 100);
    }

    private void AddQuad(float3x4 positions)
    {
        float3 normal = math.up();
        short diagonal_1 = AddVertex(positions[0], normal);
        AddVertex(positions[1], normal);
        short diagonal_2 = AddVertex(positions[2], normal);

        AddIndex(diagonal_1);
        AddIndex(diagonal_2);
        AddVertex(positions[3], normal);
    }

    private short AddVertex(float3 pos, float3 normal)
    { 
        VertexData vertex = new VertexData();
        vertex.position = pos;
        vertex.normal = normal;
        vertex.uv = _uv;
        
        OutputVertices[_totalVertexCount++] = vertex;
        short addedVertexIndex = _segmentVertexCount;
        _segmentVertexCount++;

        OutputIndices[_totalIndexCount++] = addedVertexIndex;

        return addedVertexIndex;
    }

    private void AddIndex(short vertexIndex)
    {
        OutputIndices[_totalIndexCount++] = vertexIndex;
    }
}