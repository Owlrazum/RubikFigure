using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Mathematics;
using Unity.Collections;

using Orazum.Math;
using Orazum.Meshing;
using static Orazum.Math.EasingUtilities;
using static Orazum.Math.MathUtils;

public class ValknutTransitionsTests
{
    [UnityTest]
    public IEnumerator TasToTas()
    {
        MeshDataLineSegmets originMeshData = new MeshDataLineSegmets(4);
        MeshDataLineSegmets targetMeshData = new MeshDataLineSegmets(4);

        originMeshData.LineSegments[0] = new float3x2(new float3(-1, 0, -3), new float3(-1, 0, -4));
        originMeshData.LineSegments[1] = new float3x2(new float3(-2, 0, -3), new float3(-3, 0, -4));
        originMeshData.LineSegments[2] = new float3x2(new float3(-2, 0, -1), new float3(-3, 0, -0));
        originMeshData.LineSegments[3] = new float3x2(new float3(-1, 0, -1), new float3(-1, 0, -0));


        targetMeshData.LineSegments[0] = new float3x2(new float3(1, 0, -1), new float3(0, 0, -1));
        targetMeshData.LineSegments[1] = new float3x2(new float3(1, 0, 2), new float3(0, 0, 3));
        targetMeshData.LineSegments[2] = new float3x2(new float3(2, 0, 2), new float3(3, 0, 3));
        targetMeshData.LineSegments[3] = new float3x2(new float3(2, 0, -1), new float3(3, 0, -1));

        PlayModeTestsUtils.CreateCamera(new float3(0, 10, 0), math.down(), math.forward());
        yield return Test(originMeshData, targetMeshData, TasToTasInitializer);
    }

    private void TasToTasInitializer(ref ValknutTransitionsBuilder builder)
    {
        builder.InitializeOriginRays(LineEndType.End, LineEndDirectionType.StartToEnd);
        builder.InitializeTargetRay(LineEndType.Start, LineEndDirectionType.EndToStart, LineEndType.End);
    }

    [UnityTest]
    public IEnumerator TasToOas()
    {
        MeshDataLineSegmets originMeshData = new MeshDataLineSegmets(4);
        MeshDataLineSegmets targetMeshData = new MeshDataLineSegmets(3);

        originMeshData.LineSegments[0] = new float3x2(new float3(-1, 0, 0), new float3(-1, 0, -1));
        originMeshData.LineSegments[1] = new float3x2(new float3(-2, 0, 0), new float3(-3, 0, -1));
        originMeshData.LineSegments[2] = new float3x2(new float3(-2, 0, 3), new float3(-3, 0, 4));
        originMeshData.LineSegments[3] = new float3x2(new float3(-1, 0, 3), new float3(-1, 0, 4));


        targetMeshData.LineSegments[0] = new float3x2(new float3(2, 0, -3), new float3(1, 0, -3));
        targetMeshData.LineSegments[1] = new float3x2(new float3(1, 0, -2), new float3(0, 0, -2));
        targetMeshData.LineSegments[2] = new float3x2(new float3(2, 0, 0), new float3(1, 0, 0));

        PlayModeTestsUtils.CreateCamera(new float3(0, 10, 0), math.down(), math.forward());
        yield return Test(originMeshData, targetMeshData, TasToOasInitializer);
    }

    private void TasToOasInitializer(ref ValknutTransitionsBuilder builder)
    { 

    }

    private delegate void RayInitializerDelegate(ref ValknutTransitionsBuilder builder);
    private IEnumerator Test(MeshDataLineSegmets originData, MeshDataLineSegmets targetData, RayInitializerDelegate Initializer)
    {
        QuadStrip originQs = new QuadStrip(originData.LineSegments);
        QuadStrip targetQs = new QuadStrip(targetData.LineSegments);

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);

        QuadStripBuilder builder = new QuadStripBuilder(originData.Vertices, originData.Indices, normalUV);
        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        builder.Build(originQs, ref buffersIndexers);
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter originMesh);
        MeshGenUtils.ApplyMeshBuffers(originData.Vertices, originData.Indices, originMesh, buffersIndexers);

        builder = new QuadStripBuilder(targetData.Vertices, targetData.Indices, normalUV);
        buffersIndexers.Reset();
        builder.Build(targetQs, ref buffersIndexers);
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter targetMesh);
        MeshGenUtils.ApplyMeshBuffers(targetData.Vertices, targetData.Indices, targetMesh, buffersIndexers);

        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        yield return new WaitForSeconds(0.5f);

        ValknutTransitionsBuilder transitionsBuilder = new ValknutTransitionsBuilder(originQs, targetQs);
        NativeArray<QST_Segment> writeBuffer = new NativeArray<QST_Segment>(7, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        Initializer.Invoke(ref transitionsBuilder);
        bool isSuccess = transitionsBuilder.BuildTransition(LineEndDirectionType.StartToEnd, LineEndDirectionType.StartToEnd, ref writeBuffer);
        if (!isSuccess)
        {
            originData.Dispose();
            targetData.Dispose();

            writeBuffer.Dispose();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
            yield break;
        }
        GameObject.Destroy(originMesh);
        GameObject.Destroy(targetMesh);

        QS_Transition transition = new QS_Transition(writeBuffer);
        MeshData meshData = new MeshData(10);
        QST_Animator animator = new QST_Animator(meshData.Vertices, meshData.Indices, normalUV);
        animator.AssignTransition(transition);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter mesh);
        float lerpParam = 0;
        buffersIndexers.Reset();
        while (lerpParam < 1)
        {
            lerpParam += PlayModeTestsParams.ExtraSlowLerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            MeshGenUtils.ApplyMeshBuffers(meshData.Vertices, meshData.Indices, mesh, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        originData.Dispose();
        targetData.Dispose();

        writeBuffer.Dispose();
        meshData.Dispose();
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }
}
