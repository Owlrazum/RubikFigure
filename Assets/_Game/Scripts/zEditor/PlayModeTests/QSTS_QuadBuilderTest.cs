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
    private static float LerpSpeed = 1;
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator OneQuadStrip()
    {
        NativeArray<float3x2> lineSegments = new NativeArray<float3x2>(10, Allocator.Persistent);
        NativeArray<VertexData> vertices = new NativeArray<VertexData>(lineSegments.Length * 2, Allocator.Persistent);
        NativeArray<short> indices = new NativeArray<short>((lineSegments.Length - 1) * 6, Allocator.Persistent);
        NativeArray<QST_Segment> fadeOutSegments = new NativeArray<QST_Segment>(lineSegments.Length - 1, Allocator.Persistent);
        NativeArray<QST_Segment> fadeInSegments = new NativeArray<QST_Segment>(lineSegments.Length - 1, Allocator.Persistent);

        float3x2 start = new float3x2(new float3(-2, 0, -1), new float3(-2, 0, 1));
        lineSegments[0] = start;
        float3x2 delta = new float3x2(new float3(1, 0, 0), new float3(1, 0, 0));
        float3x2 current = start + delta;
        for (int i = 1; i < lineSegments.Length; i++)
        {
            lineSegments[i] = current;
            current += delta;
        }
        QuadStrip qs = new QuadStrip(lineSegments);

        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builder = new QuadStripBuilder(vertices, indices, normalUV);
        builder.Build(qs, ref buffersIndexers);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshContainer);
        PlayModeTestsUtils.CreateCamera(new float3(2.5f, 10, 0), new float3(0, -1, 0), new float3(1, 0, 0));
        PlayModeTestsUtils.ApplyMeshBuffers(vertices, indices, meshContainer, buffersIndexers);

        QSTS_QuadBuilder transitionBuilder = new QSTS_QuadBuilder();
        transitionBuilder.BuildFadeOutTransition(qs, ref fadeOutSegments);
        transitionBuilder.BuildFadeInTransition(qs, ref fadeInSegments);
        QS_Transition fadeOutTransition = new QS_Transition(fadeOutSegments);
        QS_Transition fadeInTransition = new QS_Transition(fadeInSegments);

        yield return new WaitForSeconds(1);
        QST_Animator animator = new QST_Animator(vertices, indices, normalUV);
        animator.AssignTransition(fadeOutTransition);
        float lerpParam = 0;
        buffersIndexers.Reset();
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(vertices, indices, meshContainer, buffersIndexers);
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
            PlayModeTestsUtils.ApplyMeshBuffers(vertices, indices, meshContainer, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        lineSegments.Dispose();
        vertices.Dispose();
        indices.Dispose();
        fadeOutSegments.Dispose();
        fadeInSegments.Dispose();

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }
}
