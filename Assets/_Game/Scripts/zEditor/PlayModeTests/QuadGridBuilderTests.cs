using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
using static Orazum.Math.EasingUtilities;
using static Orazum.Math.MathUtils;

public class QuadGridBuilderTests
{
    [UnityTest]
    public IEnumerator Simple()
    {
        int2 dims = new int2(5, 5);
        NativeArray<float3> gridPos = new NativeArray<float3>(dims.x * dims.y, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        float d = 1;
        float rowStart = d * (dims.y - 1) / 2;
        float colStart = -d * (dims.x - 1) / 2;
        float3 rowDelta = new float3(0, 0, -d);
        float3 colDelta = new float3(d, 0, 0);

        float3 pos = new float3(colStart, 0, rowStart);
        int indexer = 0;
        for (int i = 0; i < dims.y; i++)
        {
            for (int j = 0; j < dims.x; j++)
            {
                gridPos[indexer++] = pos;
                pos += colDelta;
            }
            pos.x = colStart;
            pos += rowDelta;
        }

        NativeArray<VertexData> vertices = new NativeArray<VertexData>(gridPos.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<short> indices = new NativeArray<short>((dims.x - 1) * (dims.y - 1) * 6, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        QuadGridBuilder gridBuilder = new QuadGridBuilder(vertices, indices, float2.zero);

        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        NativeArray<float3> row = gridPos.GetSubArray(0, dims.x);
        gridBuilder.Start(row, ref buffersIndexers);
        for (int i = 1; i < dims.y; i++)
        {
            row = gridPos.GetSubArray(i * dims.x, dims.x);
            gridBuilder.Continue(row, ref buffersIndexers);
        }

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter mesh);
        PlayModeTestsUtils.ApplyMeshBuffers(vertices, indices, mesh, buffersIndexers);
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        PlayModeTestsUtils.CreateCamera(new float3(0, 10, 0), new float3(0, -1, 0), new float3(0, 0, 1));
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }
}
