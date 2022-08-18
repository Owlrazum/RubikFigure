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
        public float3 Up;
        public float3 Right;
        public float3 Left;

        public void Rotate(quaternion rotation)
        {
            Up = math.rotate(rotation, Up);
            Right = math.rotate(rotation, Right);
            Left = math.rotate(rotation, Left);
        }

        public void Offset(float3 offset)
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
            Up = triangleVertex,
            Right = math.rotate(_rightRotate, triangleVertex),
            Left = math.rotate(_leftRotate, triangleVertex)
        };
        return triangle;
    }

    private void DrawTriangle(Triangle triangle)
    { 
        Debug.DrawLine(triangle.Up,    triangle.Right, Color.green, 100);
        Debug.DrawLine(triangle.Right, triangle.Left, Color.green, 100);
        Debug.DrawLine(triangle.Left,  triangle.Up, Color.green, 100);
    }

    private void DrawRays(float4x3 rays)
    { 
        Debug.DrawRay(Float3(rays[0].xy), Float3(rays[0].zw) * 5, Color.red, 100);
        Debug.DrawRay(Float3(rays[1].xy), Float3(rays[1].zw) * 5, Color.red, 100);
        Debug.DrawRay(Float3(rays[2].xy), Float3(rays[2].zw) * 5, Color.red, 100);
    }

    private float3 Float3(float2 xy)
    {
        return new float3(xy.x, 0, xy.y);
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

        float4x3 upRaysCW    = CalculateCWRaysForTriangle(upTriangle);
        float4x3 rightRaysCW = CalculateCWRaysForTriangle(rightTriangle);
        float4x3 leftRaysCW  = CalculateCWRaysForTriangle(leftTriangle);
        
        DrawRays(upRaysCW);
        DrawRays(rightRaysCW);
        DrawRays(leftRaysCW);        

        float4x3 upRaysCCW    = CalculateCCWRaysForTriangle(upTriangle);
        float4x3 rightRaysCCW = CalculateCCWRaysForTriangle(rightTriangle);
        float4x3 leftRaysCCW  = CalculateCCWRaysForTriangle(leftTriangle);

        float3 intersect;

        TwoAngleSegment uus = new TwoAngleSegment();

        uus.RightQuad = new float3x4();
        uus.RightQuad[0] = upTriangle.Up;
        IntersectRays(upRaysCW[0], rightRaysCCW[0], out intersect);
        uus.RightQuad[1] = intersect + Float3(upRaysCCW[2].zw) * P_GapSize;
        uus.RightQuad[2] = uus.RightQuad[1] + Float3(rightRaysCCW[0].zw) * P_Width;
        IntersectRays(upRaysCCW[0], new float4(uus.RightQuad[2].xz, upRaysCCW[2].zw), out intersect);
        uus.RightQuad[3] = intersect + Float3(upRaysCW[0].zw) * P_Width;

        AddQuad(uus.RightQuad);
    }

    private float4x3 CalculateCWRaysForTriangle(Triangle triangle)
    { 
        float4x3 raysCW = new float4x3();
        raysCW[0] = new float4(triangle.Up.xz,    math.normalize(triangle.Right.xz - triangle.Up.xz));
        raysCW[1] = new float4(triangle.Right.xz, math.normalize(triangle.Left.xz  - triangle.Right.xz));
        raysCW[2] = new float4(triangle.Left.xz,  math.normalize(triangle.Up.xz    - triangle.Left.xz));
        return raysCW;
    }

    private float4x3 CalculateCCWRaysForTriangle(Triangle triangle)
    { 
        float4x3 raysCCW = new float4x3();
        raysCCW[0] = new float4(triangle.Up.xz,    math.normalize(triangle.Left.xz  - triangle.Up.xz));
        raysCCW[1] = new float4(triangle.Left.xz,  math.normalize(triangle.Right.xz - triangle.Left.xz));
        raysCCW[2] = new float4(triangle.Right.xz, math.normalize(triangle.Up.xz    - triangle.Right.xz));
        return raysCCW;
    }

    private struct OneAngleSegment
    {
        public float3x4 LeftQuad;
        public float3x4 RightQuad;
    }

    private struct TwoAngleSegment
    {
        public float3x4 LeftQuad;
        public float3x4 CenterQuad;
        public float3x4 RightQuad;
    }

    private void AddOneAngleSegment(OneAngleSegment oneAngleSegment)
    {
        _segmentVertexCount = 0;
        AddQuad(oneAngleSegment.LeftQuad);
        AddQuad(oneAngleSegment.RightQuad);
    }

    private void AddTwoAngleSegment(TwoAngleSegment twoAngleSegment)
    {
        _segmentVertexCount = 0;
        AddQuad(twoAngleSegment.LeftQuad);
        AddQuad(twoAngleSegment.CenterQuad);
        AddQuad(twoAngleSegment.RightQuad);
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