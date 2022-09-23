using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

using Orazum.Meshing;
using static Orazum.Math.EasingUtilities;
using static Orazum.Math.MathUtils;

public class FigureShuffleTranstionTests
{
    private const int QSLS_Count = 10;

    [UnityTest]
    public IEnumerator TestShuffleTransition()
    {
        int lineSegmentsCount = QSLS_Count * 2;
        MeshDataLineSegmets meshData = new(lineSegmentsCount);

        float3x2 startLeft = new float3x2(new float3(-2, 0, -1), new float3(-2, 0, -0.25f));
        float3x2 delta = new float3x2(new float3( 1, 0, 0), new float3( 1, 0, 0));
        float3x2 startRight = new float3x2(new float3(-2, 0, 0.25f), new float3(-2, 0, 1));
        float3x4 startLeftRight = new float3x4(startLeft[0], startLeft[1], startRight[0], startRight[1]);
        QuadStripsBuffer quadStripsBuffer = GenerateDoubleQuadStripsBuffer(startLeftRight, delta);

        MeshBuffersIndexers bi = new MeshBuffersIndexers();

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builder = new QuadStripBuilder(meshData.Vertices, meshData.Indices, normalUV);
        builder.Build(quadStripsBuffer.GetQuadStrip(0), ref bi);
        builder.Build(quadStripsBuffer.GetQuadStrip(1), ref bi);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter mesh);
        PlayModeTestsUtils.CreateCamera(new float3(2.5f, 10, 0), new float3(0, -1, 0), new float3(1, 0, 0));
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        MeshGenUtils.ApplyMeshBuffers(meshData.Vertices, meshData.Indices, mesh, bi);

        var bufferIndexers = BufferUtils.GetFadeInOutTransitionsBufferIndexers(quadStripsBuffer);

        NativeArray<QST_Segment> transSegsBuffer = new((lineSegmentsCount - 1) * 2, Allocator.Persistent);
        NativeArray<int2> transitionsIndexers = new(bufferIndexers.Length, Allocator.Persistent);
        QS_TransitionsBuffer transitionsBuffer = new QS_TransitionsBuffer(transSegsBuffer, transitionsIndexers);

        FigureUniversalTransitionsGenJob jobShuffleTrans = new FigureUniversalTransitionsGenJob()
        {
            InputQuadStripsCollection = quadStripsBuffer,
            InputQSTransitionsBufferIndexers = bufferIndexers,
            OutputQSTransitionsBuffer = transitionsBuffer
        };
        jobShuffleTrans.Run(transitionsIndexers.Length);
        yield return new WaitForSeconds(1);

        FadeOutInTransitions[] shuffleTranses = new FadeOutInTransitions[2];
        for (int i = 0; i < transitionsBuffer.TransitionsCount; i += 2)
        {
            QS_Transition fadeOut = transitionsBuffer.GetQSTransition(i);
            QS_Transition fadeIn = transitionsBuffer.GetQSTransition(i + 1);

            FadeOutInTransitions transData = new FadeOutInTransitions();
            transData.FadeOut = fadeOut;
            transData.FadeIn = fadeIn;
            shuffleTranses[i / 2] = transData;
        }

        var concBuffer = QS_Transition.PrepareConcatenationBuffer(
            shuffleTranses[0].FadeOut, shuffleTranses[1].FadeIn, Allocator.Persistent);

        var transConc = QS_Transition.Concatenate(shuffleTranses[0].FadeOut, shuffleTranses[1].FadeIn, concBuffer);

        QST_Animator animConc = new QST_Animator(meshData.Vertices, meshData.Indices, normalUV);

        animConc.AssignTransition(transConc);
        float lerpParam = 0;
        bi.Reset();
        while (lerpParam < 1)
        {
            lerpParam += PlayModeTestsParams.FastLerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            animConc.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref bi);
            MeshGenUtils.ApplyMeshBuffers(meshData.Vertices, meshData.Indices, mesh, bi);
            bi.Reset();
            yield return null;
        }

        meshData.Dispose();
        transitionsBuffer.Dispose();
        transConc.DisposeConcatenation();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
        GameObject.Destroy(mesh.gameObject);
    }

    private QuadStripsBuffer GenerateDoubleQuadStripsBuffer(in float3x4 startLeftRight, in float3x2 delta)
    {
        int indexer = 0;
        float3x2 start =  new float3x2(startLeftRight[0], startLeftRight[1]);
        NativeArray<float3x2> lineSegmentsBuffer = new NativeArray<float3x2>(QSLS_Count * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        lineSegmentsBuffer[indexer++] = start;
        float3x2 current = start + delta;
        for (int i = 1; i < QSLS_Count; i++)
        {
            lineSegmentsBuffer[indexer++] = current;
            current += delta;
        }

        start = new float3x2(startLeftRight[2], startLeftRight[3]);
        lineSegmentsBuffer[indexer++] = start;
        current = start + delta;
        for (int i = 1; i < QSLS_Count; i++)
        {
            lineSegmentsBuffer[indexer++] = current;
            current += delta;
        }

        var quadStripIndexers = new NativeArray<int2>(2, Allocator.Temp);
        quadStripIndexers[0] = new int2(0, QSLS_Count);
        quadStripIndexers[1] = new int2(QSLS_Count, QSLS_Count);
        return new QuadStripsBuffer(lineSegmentsBuffer, quadStripIndexers);
    }
}
