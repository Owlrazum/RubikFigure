using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Meshing;
using static Orazum.Math.LineSegmentUtilities;

public struct MeshDataLineSegments : IDisposable
{
    public NativeArray<float3x2> LineSegments;
    public NativeArray<VertexData> Vertices;
    public NativeArray<short> Indices;

    public MeshDataLineSegments(int lineSegmentsCount)
    {
        LineSegments = new NativeArray<float3x2>(lineSegmentsCount, Allocator.Persistent);
        Vertices = new NativeArray<VertexData>(lineSegmentsCount * 2, Allocator.Persistent);
        Indices = new NativeArray<short>((lineSegmentsCount - 1) * 6, Allocator.Persistent);
    }

    public void Dispose()
    {
        LineSegments.Dispose();
        Vertices.Dispose();
        Indices.Dispose();
    }
}

public struct MeshData : IDisposable
{
    public NativeArray<VertexData> Vertices;
    public NativeArray<short> Indices;

    public MeshData(int lineSegmentsCount)
    {
        Vertices = new NativeArray<VertexData>(lineSegmentsCount * 2, Allocator.Persistent);
        Indices = new NativeArray<short>((lineSegmentsCount - 1) * 6, Allocator.Persistent);
    }

    public void Dispose()
    {
        Vertices.Dispose();
        Indices.Dispose();
    } 
}

public struct TransitionData : IDisposable
{
    public NativeArray<VertexData> Vertices;
    public NativeArray<short> Indices;
    public NativeArray<QST_Segment> First;
    public NativeArray<QST_Segment> Second;

    public TransitionData(int lineSegmentsCount, int transSegmentsCount)
    {
        Vertices = new NativeArray<VertexData>(lineSegmentsCount * lineSegmentsCount, Allocator.Persistent);
        Indices = new NativeArray<short>((lineSegmentsCount - 1) * (lineSegmentsCount - 1) * 6, Allocator.Persistent);
        First = new NativeArray<QST_Segment>(transSegmentsCount, Allocator.Persistent);
        Second = new NativeArray<QST_Segment>(transSegmentsCount, Allocator.Persistent);
    }

    public TransitionData(in NativeArray<VertexData> vertices, in NativeArray<short> indices, int transSegmentsCount)
    {
        Vertices = vertices;
        Indices = indices;
        First = new NativeArray<QST_Segment>(transSegmentsCount, Allocator.Persistent);
        Second = new NativeArray<QST_Segment>(transSegmentsCount, Allocator.Persistent);
    }

    public void Dispose()
    {
        Vertices.Dispose();
        Indices.Dispose();
        First.Dispose();
        Second.Dispose();
    }
}

public static class MeshGenUtils
{
    public static void ApplyMeshBuffers(
            in NativeArray<VertexData> vertices,
            in NativeArray<short> indices,
            MeshFilter meshContainer,
            in MeshBuffersIndexers buffersIndexers)
    {
        Mesh mesh = meshContainer.mesh;
        mesh.MarkDynamic();

        mesh.SetVertexBufferParams(buffersIndexers.Count.x, VertexData.VertexBufferMemoryLayout);
        mesh.SetIndexBufferParams(buffersIndexers.Count.y, IndexFormat.UInt16);

        mesh.SetVertexBufferData(vertices, buffersIndexers.Start.x, 0, buffersIndexers.Count.x, 0, MeshUpdateFlags.Default);
        mesh.SetIndexBufferData(indices, buffersIndexers.Start.y, 0, buffersIndexers.Count.y, MeshUpdateFlags.Default);

        mesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: buffersIndexers.Count.y
        );
        mesh.SetSubMesh(0, subMesh);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public static QuadStrip SimpleQuadStrip(ref MeshDataLineSegments data, in float3x2 start, in float3x2 delta)
    {
        data.LineSegments[0] = start;
        float3x2 current = start + delta;
        for (int i = 1; i < data.LineSegments.Length; i++)
        {
            data.LineSegments[i] = current;
            current += delta;
        }
        return new QuadStrip(data.LineSegments);
    }

    public static QuadStrip SimpleRadialQuadStrip(ref MeshDataLineSegments data, in float3x2 start, float angleRad)
    {
        data.LineSegments[0] = start;

        quaternion q = quaternion.AxisAngle(math.up(), angleRad);
        float3x2 current = start;
        current = RotateLineSegment(q, current);
        for (int i = 1; i < data.LineSegments.Length; i++)
        {
            data.LineSegments[i] = current;
            current = RotateLineSegment(q, current);
        }
        return new QuadStrip(data.LineSegments);
    }

    public static void SquareArcMesh(
        float3 center,
        float angle,
        float width,
        float gapWidth,
        float gapHeight,
        out MeshDataLineSegments meshData
    )
    {
        float3x2 s1 = new float3x2(new float3(-gapWidth, 0, 0        ), new float3(-gapWidth - width, 0, 0));
        float3x2 s2 = new float3x2(new float3(-gapWidth, 0, gapHeight), new float3(-gapWidth - width, 0, gapHeight + width));
        float3x2 s3 = new float3x2(new float3( gapWidth, 0, gapHeight), new float3( gapWidth + width, 0, gapHeight + width));
        float3x2 s4 = new float3x2(new float3( gapWidth, 0, 0        ), new float3( gapWidth + width, 0, 0));

        quaternion q = quaternion.AxisAngle(math.up(), angle);

        s1 = RotateLineSegment(q, s1);
        s2 = RotateLineSegment(q, s2);
        s3 = RotateLineSegment(q, s3);
        s4 = RotateLineSegment(q, s4);

        float3x2 centerDelta = new float3x2(center, center);
        meshData = new MeshDataLineSegments(4);
        meshData.LineSegments[0] = s1 + centerDelta;
        meshData.LineSegments[1] = s2 + centerDelta;
        meshData.LineSegments[2] = s3 + centerDelta;
        meshData.LineSegments[3] = s4 + centerDelta;
    }
}