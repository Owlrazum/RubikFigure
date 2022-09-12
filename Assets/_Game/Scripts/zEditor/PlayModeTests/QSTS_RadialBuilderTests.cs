using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
using Orazum.Math;
using static Orazum.Math.LineSegmentUtilities;
using static QSTS_FillData;
using static Orazum.Math.EasingUtilities;
using static Orazum.Math.MathUtils;

public class QSTS_RadialBuilderTests
{
    private static float LerpSpeed = 0.5f;

    private struct NativeData : IDisposable
    { 
        public NativeArray<float3x2> LineSegments;
        public NativeArray<VertexData> Vertices;
        public NativeArray<short> Indices;
        public NativeArray<QST_Segment> CW;
        public NativeArray<QST_Segment> AntiCW;

        public NativeData(int lineSegmentsCount, int transSegmentsCount)
        { 
            LineSegments = new NativeArray<float3x2>(lineSegmentsCount, Allocator.Persistent);
            Vertices = new NativeArray<VertexData>(lineSegmentsCount * 2, Allocator.Persistent);
            Indices = new NativeArray<short>((lineSegmentsCount - 1) * 6, Allocator.Persistent);
            CW = new NativeArray<QST_Segment>(transSegmentsCount, Allocator.Persistent);
            AntiCW = new NativeArray<QST_Segment>(transSegmentsCount, Allocator.Persistent);
        }

        public void Dispose()
        { 
            LineSegments.Dispose();
            Vertices.Dispose();
            Indices.Dispose();
            CW.Dispose();
            AntiCW.Dispose();
        }
    }

    [UnityTest]
    public IEnumerator OneQuadStrip()
    {
        int resolution = 18;
        NativeData data = new NativeData(resolution + 1, 1);

        float3x2 start = new float3x2(new float3(0, 0, 1), new float3(0, 0, 2));
        float angleRad = math.radians(5);
        QuadStrip qs = GenerateSimpleQuadStrip(ref data, start, angleRad);

        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builder = new QuadStripBuilder(data.Vertices, data.Indices, normalUV);
        builder.Build(qs, ref buffersIndexers);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshContainer);
        PlayModeTestsUtils.CreateCamera(new float3(0, 10, 0), math.down(), math.forward());
        PlayModeTestsUtils.ApplyMeshBuffers(data.Vertices, data.Indices, meshContainer, buffersIndexers);

        QSTS_RadialBuilder radialBuilder = new QSTS_RadialBuilder();
        radialBuilder = new QSTS_RadialBuilder(math.up(), angleRad, resolution);

        radialBuilder.FillIn_SRL(qs, new float2(0, 1), ClockOrderType.CW, out QST_Segment qsts);
        data.CW[0] = qsts;

        radialBuilder.FillOut_SRL(qs, new float2(0, 1), ClockOrderType.CW, out qsts);
        data.AntiCW[0] = qsts;

        QS_Transition fadeInTransition = new QS_Transition(data.CW);
        QS_Transition fadeOutTransition = new QS_Transition(data.AntiCW);

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
        GameObject.Destroy(meshContainer.gameObject);
    }

    private QuadStrip GenerateSimpleQuadStrip(ref NativeData data, in float3x2 start, float angleRad)
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
}
