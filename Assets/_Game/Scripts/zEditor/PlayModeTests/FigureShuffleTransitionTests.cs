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

// TODO Move transition indexing generation out of specific classes.
// CustomShuffle redo

public class FigureShuffleTranstionTests
{
    private static float LerpSpeed = 0.2f;
    private static int QSLS_Count = 10;

    private struct NativeData : IDisposable
    { 
        public NativeArray<float3x2> LineSegmentsBuffer;
        public NativeArray<VertexData> Vertices;
        public NativeArray<short> Indices;
        public NativeArray<QST_Segment> TransitionSegmentsBuffer;
        
        public NativeArray<int2> QuadStripsIndexers;
        public NativeArray<int2> TransitionsIndexers;

        public NativeData(int lineSegmentsCount)
        { 
            LineSegmentsBuffer = new NativeArray<float3x2>(lineSegmentsCount, Allocator.Persistent);
            Vertices = new NativeArray<VertexData>(lineSegmentsCount * 2, Allocator.Persistent);
            Indices = new NativeArray<short>((lineSegmentsCount - 1) * 6, Allocator.Persistent);
            TransitionSegmentsBuffer = new NativeArray<QST_Segment>((lineSegmentsCount - 1) * 2, Allocator.Persistent);
            
            QuadStripsIndexers = new();
            TransitionsIndexers = new();
        }

        public void Dispose()
        { 
            LineSegmentsBuffer.Dispose();
            Vertices.Dispose();
            Indices.Dispose();
            TransitionSegmentsBuffer.Dispose();
            QuadStripsIndexers.Dispose();
            TransitionsIndexers.Dispose();
        }
    }

    [UnityTest]
    public IEnumerator TestShuffleTransition()
    { 
        NativeData data = new NativeData(QSLS_Count * 2);

        float3x2 startLeft = new float3x2(new float3(-2, 0, -1), new float3(-2, 0, -0.25f));
        float3x2 delta = new float3x2(new float3( 1, 0, 0), new float3( 1, 0, 0));
        float3x2 startRight = new float3x2(new float3(-2, 0, 0.25f), new float3(-2, 0, 1));
        float3x4 startLeftRight = new float3x4(startLeft[0], startLeft[1], startRight[0], startRight[1]);
        QuadStripsBuffer quadStripsBuffer = GenerateDoubleQuadStripsBuffer(ref data, startLeftRight, delta);

        MeshBuffersIndexers bi = new MeshBuffersIndexers();

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builder = new QuadStripBuilder(data.Vertices, data.Indices, normalUV);
        builder.Build(quadStripsBuffer.GetQuadStrip(0), ref bi);
        builder.Build(quadStripsBuffer.GetQuadStrip(1), ref bi);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter mesh);
        PlayModeTestsUtils.CreateCamera(new float3(2.5f, 10, 0), new float3(0, -1, 0), new float3(1, 0, 0));
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        PlayModeTestsUtils.ApplyMeshBuffers(data.Vertices, data.Indices, mesh, bi);

        var bufferIndexers = BufferUtils.GetFadeInOutTransitionsBufferIndexers(quadStripsBuffer);
        data.TransitionsIndexers = new(bufferIndexers.Length, Allocator.Persistent);
        QS_TransitionsBuffer transitionsBuffer = new QS_TransitionsBuffer(data.TransitionSegmentsBuffer, data.TransitionsIndexers);

        FigureUniversalTransitionsGenJob jobShuffleTrans = new FigureUniversalTransitionsGenJob()
        {
            InputQuadStripsCollection = quadStripsBuffer,
            InputQSTransitionsBufferIndexers = bufferIndexers,
            OutputQSTransitionsBuffer = transitionsBuffer
        };
        jobShuffleTrans.Run(data.TransitionsIndexers.Length);
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

        QST_Animator animConc = new QST_Animator(data.Vertices, data.Indices, normalUV);

        animConc.AssignTransition(transConc);
        float lerpParam = 0;
        bi.Reset();
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            animConc.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref bi);
            PlayModeTestsUtils.ApplyMeshBuffers(data.Vertices, data.Indices, mesh, bi);
            bi.Reset();
            yield return null;
        }

        data.Dispose();
        transConc.DisposeConcatenation();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
        GameObject.Destroy(mesh.gameObject);
    }

    private QuadStripsBuffer GenerateDoubleQuadStripsBuffer(ref NativeData data, in float3x4 startLeftRight, in float3x2 delta)
    {
        int indexer = 0;
        float3x2 start =  new float3x2(startLeftRight[0], startLeftRight[1]);
        data.LineSegmentsBuffer[indexer++] = start;
        float3x2 current = start + delta;
        for (int i = 1; i < QSLS_Count; i++)
        {
            data.LineSegmentsBuffer[indexer++] = current;
            current += delta;
        }

        start = new float3x2(startLeftRight[2], startLeftRight[3]);
        data.LineSegmentsBuffer[indexer++] = start;
        current = start + delta;
        for (int i = 1; i < QSLS_Count; i++)
        {
            data.LineSegmentsBuffer[indexer++] = current;
            current += delta;
        }

        data.QuadStripsIndexers = new NativeArray<int2>(2, Allocator.Persistent);
        data.QuadStripsIndexers[0] = new int2(0, QSLS_Count);
        data.QuadStripsIndexers[1] = new int2(QSLS_Count, QSLS_Count);
        return new QuadStripsBuffer(data.LineSegmentsBuffer, data.QuadStripsIndexers);
    }
}
