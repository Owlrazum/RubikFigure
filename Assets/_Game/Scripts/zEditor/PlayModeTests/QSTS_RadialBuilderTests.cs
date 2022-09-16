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
using static Orazum.Constants.Math;

public class QSTS_RadialBuilderTests
{
    private static float LerpSpeed = 0.5f;

    private struct TransitionData : IDisposable
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

        public void Dispose()
        {
            Vertices.Dispose();
            Indices.Dispose();
            First.Dispose();
            Second.Dispose();
        }
    }

    private struct MeshData : IDisposable
    {
        public NativeArray<float3x2> LineSegments;
        public NativeArray<VertexData> Vertices;
        public NativeArray<short> Indices;

        public MeshData(int lineSegmentsCount)
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

    private TransitionData _transitionData;

    private QSTS_RadialBuilder _radialBuilder;
    private QST_Animator _animator;

    #region SingleRotationLerp
    [UnityTest]
    public IEnumerator SingleRotationLerp()
    {
        int resolution = 18;
        MeshData meshData = new MeshData(resolution + 1);

        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();

        float3x2 start = new float3x2(new float3(0, 0, 1), new float3(0, 0, 2));
        float totalAngle = TAU / 4;
        float deltaAngle = totalAngle / (resolution);
        QuadStrip quadStrip = GenerateSimpleQuadStrip(ref meshData, start, deltaAngle);

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);

        _transitionData = new TransitionData(resolution + 1, 1);
        _radialBuilder = new QSTS_RadialBuilder(math.up(), new float2(totalAngle, 0), resolution);
        _animator = new QST_Animator(_transitionData.Vertices, _transitionData.Indices, normalUV);

        QuadStripBuilder builder = new QuadStripBuilder(_transitionData.Vertices, _transitionData.Indices, normalUV);
        builder.Build(quadStrip, ref buffersIndexers);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter staticMesh);
        PlayModeTestsUtils.CreateCamera(new float3(0, 10, 0), math.down(), math.forward());
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        PlayModeTestsUtils.ApplyMeshBuffers(_transitionData.Vertices, _transitionData.Indices, staticMesh, buffersIndexers);


        yield return new WaitForSeconds(0.5f);
        GameObject.Destroy(staticMesh.gameObject);
        yield return TestClockwise_SRL(quadStrip);
        yield return TestAntiClockwise_SRL(quadStrip);

        _transitionData.Dispose();
        meshData.Dispose();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }

    private IEnumerator TestClockwise_SRL(QuadStrip quadStrip)
    {
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter dynamicMesh);

        _radialBuilder.FillIn_SRL(quadStrip, lerpRange: new float2(0, 1), lerpLength: 1, isNew: true, isTemporary: false, ClockOrderType.CW, out QST_Segment qsts);
        _transitionData.First[0] = qsts;
        QS_Transition fadeInTransition = new QS_Transition(_transitionData.First);

        _radialBuilder.FillOut_SRL(quadStrip, lerpRange: new float2(0, 1), lerpLength: 1, isNew: true, ClockOrderType.CW, out qsts);
        _transitionData.Second[0] = qsts;
        QS_Transition fadeOutTransition = new QS_Transition(_transitionData.Second);

        _animator.AssignTransition(fadeInTransition);
        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        float lerpParam = 0;
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            _animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(_transitionData.Vertices, _transitionData.Indices, dynamicMesh, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        _animator.AssignTransition(fadeOutTransition);
        lerpParam = 0;
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            _animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(_transitionData.Vertices, _transitionData.Indices, dynamicMesh, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        GameObject.Destroy(dynamicMesh.gameObject);
    }
    private IEnumerator TestAntiClockwise_SRL(QuadStrip quadStrip)
    {
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter dynamicMesh);

        _radialBuilder.FillIn_SRL(quadStrip, lerpRange: new float2(0, 1), lerpLength: 1, isNew: true, isTemporary: false, ClockOrderType.AntiCW, out QST_Segment qsts);
        _transitionData.First[0] = qsts;
        QS_Transition fadeInTransition = new QS_Transition(_transitionData.First);

        _radialBuilder.FillOut_SRL(quadStrip, lerpRange: new float2(0, 1), lerpLength: 1, isNew: true, ClockOrderType.AntiCW, out qsts);
        _transitionData.Second[0] = qsts;
        QS_Transition fadeOutTransition = new QS_Transition(_transitionData.Second);

        yield return new WaitForSeconds(0.5f);

        _animator.AssignTransition(fadeInTransition);
        float lerpParam = 0;
        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            _animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(_transitionData.Vertices, _transitionData.Indices, dynamicMesh, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        _animator.AssignTransition(fadeOutTransition);
        lerpParam = 0;
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            _animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(_transitionData.Vertices, _transitionData.Indices, dynamicMesh, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        GameObject.Destroy(dynamicMesh.gameObject);
    }
    #endregion

    #region DoubleRotationLerp
    [UnityTest]
    public IEnumerator DoubleRotationLerp()
    {
        int resolution = 15;
        MeshData meshDataUp = new MeshData(resolution + 1);
        MeshData meshDataDown = new MeshData(resolution + 1);
        MeshBuffersIndexers buffersIndexersUp = new MeshBuffersIndexers();
        MeshBuffersIndexers buffersIndexersDown = new MeshBuffersIndexers();

        float totalAngle = TAU / 4;
        float deltaAngle = totalAngle / resolution;
        float3x2 startUp = new float3x2(new float3(0, 0, 3), new float3(0, 0, 4));
        QuadStrip quadStripUp = GenerateSimpleQuadStrip(ref meshDataUp, startUp, deltaAngle);

        float3x2 startDown = new float3x2(new float3(0, 0, 1), new float3(0, 0, 2));
        QuadStrip quadStripDown = GenerateSimpleQuadStrip(ref meshDataDown, startDown, deltaAngle);

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builder = new QuadStripBuilder(meshDataUp.Vertices, meshDataUp.Indices, normalUV);
        builder.Build(quadStripUp, ref buffersIndexersUp);
        builder = new QuadStripBuilder(meshDataDown.Vertices, meshDataDown.Indices, normalUV);
        builder.Build(quadStripDown, ref buffersIndexersDown);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshUp);
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshDown);
        PlayModeTestsUtils.CreateCamera(new float3(2, 13, 0), math.down(), math.forward());
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        PlayModeTestsUtils.ApplyMeshBuffers(meshDataUp.Vertices, meshDataUp.Indices, meshUp, buffersIndexersUp);
        PlayModeTestsUtils.ApplyMeshBuffers(meshDataDown.Vertices, meshDataDown.Indices, meshDown, buffersIndexersDown);

        float2 angles = new float2(totalAngle, TAU / 2);

        _transitionData = new TransitionData((resolution + 1) * 2, transSegmentsCount: 3);
        _radialBuilder = new QSTS_RadialBuilder(math.up(), angles, resolution);
        _animator = new QST_Animator(_transitionData.Vertices, _transitionData.Indices, normalUV);

        yield return new WaitForSeconds(0.5f);
        GameObject.Destroy(meshUp.gameObject);
        GameObject.Destroy(meshDown.gameObject);
        yield return TestUp_DRL(quadStripDown, quadStripUp);
        //syield return TestDown_DRL(quadStripDown, quadStripUp);

        _transitionData.Dispose();
        meshDataUp.Dispose();
        meshDataDown.Dispose();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }

    private IEnumerator TestUp_DRL(QuadStrip down, QuadStrip up)
    {
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshUp);
        GetLerpPointsForLevitation(down, up, out float2 lerpRange, out float lerpLength);

        _radialBuilder.GenerateSingleMoveLerp(down, new float2(0, lerpRange.x), lerpLength: 1, isNew: true, FillType.ToStart, out QST_Segment s1);
        _radialBuilder.FillIn_DRL(down, up, new float2(0, 1), lerpLength, isNew: true, isTemporary: true, VertOrderType.Up, out QST_Segment s2);
        _radialBuilder.GenerateSingleMoveLerp(up, new float2(lerpRange.y, 1), lerpLength: 1, isNew: true, FillType.FromEnd, out QST_Segment s3);
        _transitionData.First[0] = s1;
        _transitionData.First[1] = s2;
        _transitionData.First[2] = s3;

        QS_Transition upTransition = new QS_Transition(_transitionData.First);

        _animator.AssignTransition(upTransition);
        float lerpParam = 0;
        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers(); ;
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            _animator.UpdateWithLerpPos(lerpParam, shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(_transitionData.Vertices, _transitionData.Indices, meshUp, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        yield return new WaitForSeconds(10);
        GameObject.Destroy(meshUp);
    }

    private IEnumerator TestDown_DRL(QuadStrip down, QuadStrip up)
    {
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshDown);
        GetLerpPointsForLevitation(down, up, out float2 lerpRange, out float lerpLength);

        _radialBuilder.FillIn_DRL(down, up, new float2(0, 1), lerpLength, isNew: true, isTemporary: true, VertOrderType.Down, out QST_Segment qsts);
        _transitionData.First[0] = qsts;
        QS_Transition downTransition = new QS_Transition(_transitionData.First);

        _animator.AssignTransition(downTransition);
        float lerpParam = 0;
        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers(); ;
        while (lerpParam < 1)
        {
            lerpParam += LerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            _animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            PlayModeTestsUtils.ApplyMeshBuffers(_transitionData.Vertices, _transitionData.Indices, meshDown, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        GameObject.Destroy(meshDown);
    }

    private void GetLerpPointsForLevitation(
        in QuadStrip down,
        in QuadStrip up,
        out float2 lerpRange,
        out float lerpLength
    )
    {
        float moveDistance = DistanceLineSegment(down[0][0], down[0][1]);
        float radius = DistanceLineSegment(down[0][0], up[0][1]) / 2;
        float levitationDistance = radius * TAU / 2;

        float totalDistance = moveDistance + levitationDistance + moveDistance;

        float start = moveDistance / totalDistance;
        float end = start + levitationDistance / totalDistance;
        lerpRange = new float2(start, end);
        lerpLength = moveDistance / totalDistance;
    }
    #endregion

    #region MoveLerp
    #endregion // MoveLerp
    [UnityTest]
    public IEnumerator MoveLerp()
    {
        int resolution = 15;
        MeshData meshDataUp = new MeshData(resolution + 1);
        MeshData meshDataDown = new MeshData(resolution + 1);
        MeshBuffersIndexers buffersIndexersUp = new MeshBuffersIndexers();
        MeshBuffersIndexers buffersIndexersDown = new MeshBuffersIndexers();

        float totalAngle = TAU / 4;
        float deltaAngle = totalAngle / resolution;
        float3x2 startUp = new float3x2(new float3(0, 0, 3), new float3(0, 0, 4));
        QuadStrip quadStripUp = GenerateSimpleQuadStrip(ref meshDataUp, startUp, deltaAngle);

        float3x2 startDown = new float3x2(new float3(0, 0, 1), new float3(0, 0, 2));
        QuadStrip quadStripDown = GenerateSimpleQuadStrip(ref meshDataDown, startDown, deltaAngle);

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builder = new QuadStripBuilder(meshDataUp.Vertices, meshDataUp.Indices, normalUV);
        builder.Build(quadStripUp, ref buffersIndexersUp);
        builder = new QuadStripBuilder(meshDataDown.Vertices, meshDataDown.Indices, normalUV);
        builder.Build(quadStripDown, ref buffersIndexersDown);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshUp);
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshDown);
        PlayModeTestsUtils.CreateCamera(new float3(2, 13, 0), math.down(), math.forward());
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        PlayModeTestsUtils.ApplyMeshBuffers(meshDataUp.Vertices, meshDataUp.Indices, meshUp, buffersIndexersUp);
        PlayModeTestsUtils.ApplyMeshBuffers(meshDataDown.Vertices, meshDataDown.Indices, meshDown, buffersIndexersDown);

        float2 angles = new float2(totalAngle, TAU / 2);

        _transitionData = new TransitionData((resolution + 1) * 2, 1);
        _radialBuilder = new QSTS_RadialBuilder(math.up(), angles, resolution);
        _animator = new QST_Animator(_transitionData.Vertices, _transitionData.Indices, normalUV);

        yield return new WaitForSeconds(0.5f);
        GameObject.Destroy(meshUp.gameObject);
        GameObject.Destroy(meshDown.gameObject);
        yield return TestUp_DRL(quadStripDown, quadStripUp);
        yield return TestDown_DRL(quadStripDown, quadStripUp);

        _transitionData.Dispose();
        meshDataUp.Dispose();
        meshDataDown.Dispose();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }

    private QuadStrip GenerateSimpleQuadStrip(ref MeshData data, in float3x2 start, float angleRad)
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
