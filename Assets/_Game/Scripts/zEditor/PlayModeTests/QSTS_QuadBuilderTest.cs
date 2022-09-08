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

public class QuadBulderTest
{
    private static float LerpSpeed = 1f;

    private struct NativeData : IDisposable
    { 
        public NativeArray<float3x2> LineSegments;
        public NativeArray<VertexData> Vertices;
        public NativeArray<short> Indices;
        public NativeArray<QST_Segment> FadeOutSegments;
        public NativeArray<QST_Segment> FadeInSegments;

        public NativeData(int lineSegmentsCount)
        { 
            LineSegments = new NativeArray<float3x2>(lineSegmentsCount, Allocator.Persistent);
            Vertices = new NativeArray<VertexData>(lineSegmentsCount * 2, Allocator.Persistent);
            Indices = new NativeArray<short>((lineSegmentsCount - 1) * 6, Allocator.Persistent);
            FadeOutSegments = new NativeArray<QST_Segment>(lineSegmentsCount - 1, Allocator.Persistent);
            FadeInSegments = new NativeArray<QST_Segment>(lineSegmentsCount - 1, Allocator.Persistent);
        }

        public void Dispose()
        { 
            LineSegments.Dispose();
            Vertices.Dispose();
            Indices.Dispose();
            FadeOutSegments.Dispose();
            FadeInSegments.Dispose();
        }
    }

    [UnityTest]
    public IEnumerator OneQuadStrip()
    {
        NativeData data = new NativeData(10);

        float3x2 start = new float3x2(new float3(-2, 0, -1), new float3(-2, 0, 1));
        float3x2 delta = new float3x2(new float3(1, 0, 0), new float3(1, 0, 0));
        QuadStrip qs = GenerateSimpleQuadStrip(ref data, start, delta);

        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builder = new QuadStripBuilder(data.Vertices, data.Indices, normalUV);
        builder.Build(qs, ref buffersIndexers);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshContainer);
        PlayModeTestsUtils.CreateCamera(new float3(2.5f, 10, 0), new float3(0, -1, 0), new float3(1, 0, 0));
        PlayModeTestsUtils.ApplyMeshBuffers(data.Vertices, data.Indices, meshContainer, buffersIndexers);

        QSTS_QuadBuilder transitionBuilder = new QSTS_QuadBuilder();
        transitionBuilder.BuildFadeOutTransition(qs, ref data.FadeOutSegments);
        transitionBuilder.BuildFadeInTransition(qs, ref data.FadeInSegments);
        QS_Transition fadeOutTransition = new QS_Transition(data.FadeOutSegments);
        QS_Transition fadeInTransition = new QS_Transition(data.FadeInSegments);

        yield return new WaitForSeconds(1);
        QST_Animator animator = new QST_Animator(data.Vertices, data.Indices, normalUV);
        animator.AssignTransition(fadeOutTransition);
        float lerpParam = 0;
        buffersIndexers.Reset();
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(data.Vertices, data.Indices, meshContainer, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        animator.AssignTransition(fadeInTransition);
        lerpParam = 0;
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(data.Vertices, data.Indices, meshContainer, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        data.Dispose();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }

    [UnityTest]
    public IEnumerator TwoQuadStrips()
    {
        int lineSegmentsCount = 10;
        NativeData dataLeft_ = new NativeData(lineSegmentsCount);
        NativeData dataRight = new NativeData(lineSegmentsCount);

        float3x2 startLeft_ = new float3x2(new float3(-2, 0, -1), new float3(-2, 0, -0.25f));
        float3x2 deltaLeft_ = new float3x2(new float3( 1, 0, 0), new float3( 1, 0, 0));

        // float rightStartX = -2 + (lineSegmentsCount - 1) * deltaLeft_[0].x;
        float3x2 startRight = new float3x2(new float3(-2, 0, 0.25f), new float3(-2, 0, 1));
        // float3x2 deltaRight = new float3x2(new float3(-1, 0, 0), new float3(-1, 0, 0));
        QuadStrip qsLeft_ = GenerateSimpleQuadStrip(ref dataLeft_, startLeft_, deltaLeft_);
        QuadStrip qsRight = GenerateSimpleQuadStrip(ref dataRight, startRight, deltaLeft_);

        MeshBuffersIndexers biLeft_ = new MeshBuffersIndexers();
        MeshBuffersIndexers biRight = new MeshBuffersIndexers();

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builderLeft_ = new QuadStripBuilder(dataLeft_.Vertices, dataLeft_.Indices, normalUV);
        QuadStripBuilder builderRight = new QuadStripBuilder(dataRight.Vertices, dataRight.Indices, normalUV);
        builderLeft_.Build(qsLeft_, ref biLeft_);
        builderRight.Build(qsRight, ref biRight);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshLeft_);
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshRight);
        PlayModeTestsUtils.CreateCamera(new float3(2.5f, 10, 0), new float3(0, -1, 0), new float3(1, 0, 0));
        PlayModeTestsUtils.ApplyMeshBuffers(dataLeft_.Vertices, dataLeft_.Indices, meshLeft_, biLeft_);
        PlayModeTestsUtils.ApplyMeshBuffers(dataRight.Vertices, dataRight.Indices, meshRight, biRight);

        QSTS_QuadBuilder transitionBuilder = new QSTS_QuadBuilder();
        transitionBuilder.BuildFadeOutTransition(qsLeft_, ref dataLeft_.FadeOutSegments);
        transitionBuilder.BuildFadeInTransition(qsRight, ref dataRight.FadeInSegments);
        QS_Transition fadeOutTransition = new QS_Transition(dataLeft_.FadeOutSegments);
        QS_Transition fadeInTransition = new QS_Transition(dataRight.FadeInSegments);

        yield return new WaitForSeconds(1);
        QST_Animator animLeft_ = new QST_Animator(dataLeft_.Vertices, dataLeft_.Indices, normalUV);
        QST_Animator animRight = new QST_Animator(dataRight.Vertices, dataRight.Indices, normalUV);

        animLeft_.AssignTransition(fadeOutTransition);
        animRight.AssignTransition(fadeInTransition);
        float lerpParam = 0;
        biLeft_.Reset();
        biRight.Reset();
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            float easedLerp = EaseOut(lerpParam);
            animLeft_.UpdateWithLerpPos(easedLerp, shouldReorientVertices: false, ref biLeft_);
            animRight.UpdateWithLerpPos(easedLerp, shouldReorientVertices: false, ref biRight);
            PlayModeTestsUtils.ApplyMeshBuffers(dataLeft_.Vertices, dataLeft_.Indices, meshLeft_, biLeft_);
            PlayModeTestsUtils.ApplyMeshBuffers(dataRight.Vertices, dataRight.Indices, meshRight, biRight);
            biLeft_.Reset();
            biRight.Reset();
            yield return null;
        }

        dataLeft_.Dispose();
        dataRight.Dispose();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }

    private QuadStrip GenerateSimpleQuadStrip(ref NativeData data, in float3x2 start, in float3x2 delta)
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
}
