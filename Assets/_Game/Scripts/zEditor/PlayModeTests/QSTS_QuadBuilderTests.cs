using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
using static Orazum.Math.EasingUtilities;
using static Orazum.Math.MathUtils;


// TODO: tidy up and refactor previous tests, currently invalid tests
public class QSTS_QuadBulderTests
{
    [UnityTest]
    public IEnumerator OneQuadStrip()
    {
        int lineSegmentsCount = 10;
        MeshDataLineSegmets data = new MeshDataLineSegmets(lineSegmentsCount);

        float3x2 start = new float3x2(new float3(-2, 0, -1), new float3(-2, 0, 1));
        float3x2 delta = new float3x2(new float3(1, 0, 0), new float3(1, 0, 0));
        QuadStrip qs = MeshGenUtils.GenerateSimpleQuadStrip(ref data, start, delta);

        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builder = new QuadStripBuilder(data.Vertices, data.Indices, normalUV);
        builder.Build(qs, ref buffersIndexers);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshContainer);
        PlayModeTestsUtils.CreateCamera(new float3(2.5f, 10, 0), new float3(0, -1, 0), new float3(1, 0, 0));
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        MeshGenUtils.ApplyMeshBuffers(data.Vertices, data.Indices, meshContainer, buffersIndexers);

        QSTS_QuadBuilder transitionBuilder = new QSTS_QuadBuilder();
        NativeArray<QST_Segment> FadeOut_QSTS = new NativeArray<QST_Segment>(lineSegmentsCount - 1, Allocator.Persistent);
        NativeArray<QST_Segment> FadeIn_QSTS = new NativeArray<QST_Segment>(lineSegmentsCount - 1, Allocator.Persistent);
        transitionBuilder.BuildFadeOutTransition(qs, ref FadeOut_QSTS);
        transitionBuilder.BuildFadeInTransition(qs, ref FadeIn_QSTS);
        QS_Transition fadeOutTransition = new QS_Transition(FadeOut_QSTS);
        QS_Transition fadeInTransition = new QS_Transition(FadeIn_QSTS);

        yield return new WaitForSeconds(1);
        QST_Animator animator = new QST_Animator(data.Vertices, data.Indices, normalUV);
        animator.AssignTransition(fadeOutTransition);
        float lerpParam = 0;
        buffersIndexers.Reset();
        while (lerpParam < 1)
        {
            lerpParam += PlayModeTestsParams.FastLerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            MeshGenUtils.ApplyMeshBuffers(data.Vertices, data.Indices, meshContainer, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        animator.AssignTransition(fadeInTransition);
        lerpParam = 0;
        while (lerpParam < 1)
        {
            lerpParam += PlayModeTestsParams.FastLerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            animator.UpdateWithLerpPos(EaseOut(lerpParam), shouldReorientVertices: false, ref buffersIndexers);
            MeshGenUtils.ApplyMeshBuffers(data.Vertices, data.Indices, meshContainer, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }

        data.Dispose();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
        GameObject.Destroy(meshContainer.gameObject);
    }

    [UnityTest]
    public IEnumerator TwoQuadStrips()
    {
        int lineSegmentsCount = 10;
        MeshDataLineSegmets dataLeft_ = new MeshDataLineSegmets(lineSegmentsCount);
        MeshDataLineSegmets dataRight = new MeshDataLineSegmets(lineSegmentsCount);

        float3x2 startLeft_ = new float3x2(new float3(-2, 0, -1), new float3(-2, 0, -0.25f));
        float3x2 deltaLeft_ = new float3x2(new float3( 1, 0, 0), new float3( 1, 0, 0));

        // float rightStartX = -2 + (lineSegmentsCount - 1) * deltaLeft_[0].x;
        float3x2 startRight = new float3x2(new float3(-2, 0, 0.25f), new float3(-2, 0, 1));
        // float3x2 deltaRight = new float3x2(new float3(-1, 0, 0), new float3(-1, 0, 0));
        QuadStrip qsLeft_ = MeshGenUtils.GenerateSimpleQuadStrip(ref dataLeft_, startLeft_, deltaLeft_);
        QuadStrip qsRight = MeshGenUtils.GenerateSimpleQuadStrip(ref dataRight, startRight, deltaLeft_);

        MeshBuffersIndexers biLeft_ = new MeshBuffersIndexers();
        MeshBuffersIndexers biRight = new MeshBuffersIndexers();

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builderLeft_ = new QuadStripBuilder(dataLeft_.Vertices, dataLeft_.Indices, normalUV);
        QuadStripBuilder builderRight = new QuadStripBuilder(dataRight.Vertices, dataRight.Indices, normalUV);
        builderLeft_.Build(qsLeft_, ref biLeft_);
        builderRight.Build(qsRight, ref biRight);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshLeft_);
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshRight);
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        PlayModeTestsUtils.CreateCamera(new float3(2.5f, 10, 0), new float3(0, -1, 0), new float3(1, 0, 0));
        MeshGenUtils.ApplyMeshBuffers(dataLeft_.Vertices, dataLeft_.Indices, meshLeft_, biLeft_);
        MeshGenUtils.ApplyMeshBuffers(dataRight.Vertices, dataRight.Indices, meshRight, biRight);

        QSTS_QuadBuilder transitionBuilder = new QSTS_QuadBuilder();
        NativeArray<QST_Segment> LeftFadeOut_QSTS = new NativeArray<QST_Segment>(lineSegmentsCount - 1, Allocator.Persistent);
        NativeArray<QST_Segment> RightFadeIn_QSTS = new NativeArray<QST_Segment>(lineSegmentsCount - 1, Allocator.Persistent);

        transitionBuilder.BuildFadeOutTransition(qsLeft_, ref LeftFadeOut_QSTS);
        transitionBuilder.BuildFadeInTransition(qsRight, ref RightFadeIn_QSTS);
        QS_Transition fadeOutTransition = new QS_Transition(LeftFadeOut_QSTS);
        QS_Transition fadeInTransition = new QS_Transition(RightFadeIn_QSTS);

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
            lerpParam += PlayModeTestsParams.FastLerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            float easedLerp = EaseOut(lerpParam);
            animLeft_.UpdateWithLerpPos(easedLerp, shouldReorientVertices: false, ref biLeft_);
            animRight.UpdateWithLerpPos(easedLerp, shouldReorientVertices: false, ref biRight);
            MeshGenUtils.ApplyMeshBuffers(dataLeft_.Vertices, dataLeft_.Indices, meshLeft_, biLeft_);
            MeshGenUtils.ApplyMeshBuffers(dataRight.Vertices, dataRight.Indices, meshRight, biRight);
            biLeft_.Reset();
            biRight.Reset();
            yield return null;
        }

        dataLeft_.Dispose();
        dataRight.Dispose();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
        GameObject.Destroy(meshLeft_.gameObject);
        GameObject.Destroy(meshRight.gameObject);
    }

    [UnityTest]
    public IEnumerator TransitionConcaternation()
    { 
        int lineSegmentsCount = 10;
        MeshDataLineSegmets meshDataLeft_ = new MeshDataLineSegmets(lineSegmentsCount);
        MeshDataLineSegmets meshDataRight = new MeshDataLineSegmets(lineSegmentsCount);

        float3x2 startLeft_ = new float3x2(new float3(-2, 0, -1), new float3(-2, 0, -0.25f));
        float3x2 deltaLeft_ = new float3x2(new float3( 1, 0, 0), new float3( 1, 0, 0));

        // float rightStartX = -2 + (lineSegmentsCount - 1) * deltaLeft_[0].x;
        float3x2 startRight = new float3x2(new float3(-2, 0, 0.25f), new float3(-2, 0, 1));
        // float3x2 deltaRight = new float3x2(new float3(-1, 0, 0), new float3(-1, 0, 0));
        QuadStrip qsLeft_ = MeshGenUtils.GenerateSimpleQuadStrip(ref meshDataLeft_, startLeft_, deltaLeft_);
        QuadStrip qsRight = MeshGenUtils.GenerateSimpleQuadStrip(ref meshDataRight, startRight, deltaLeft_);

        MeshBuffersIndexers biLeft_ = new MeshBuffersIndexers();
        MeshBuffersIndexers biRight = new MeshBuffersIndexers();

        float3x2 normalUV = new float3x2(new float3(0, 1, 0), float3.zero);
        QuadStripBuilder builderLeft_ = new QuadStripBuilder(meshDataLeft_.Vertices, meshDataLeft_.Indices, normalUV);
        QuadStripBuilder builderRight = new QuadStripBuilder(meshDataRight.Vertices, meshDataRight.Indices, normalUV);
        builderLeft_.Build(qsLeft_, ref biLeft_);
        builderRight.Build(qsRight, ref biRight);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshLeft_);
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter meshRight);
        PlayModeTestsUtils.CreateLight(new float3(0, -1, 1), math.forward());
        PlayModeTestsUtils.CreateCamera(new float3(2.5f, 10, 0), new float3(0, -1, 0), new float3(1, 0, 0));
        MeshGenUtils.ApplyMeshBuffers(meshDataLeft_.Vertices, meshDataLeft_.Indices, meshLeft_, biLeft_);
        MeshGenUtils.ApplyMeshBuffers(meshDataRight.Vertices, meshDataRight.Indices, meshRight, biRight);

        QSTS_QuadBuilder transitionBuilder = new QSTS_QuadBuilder();
        NativeArray<QST_Segment> LeftFadeOut_QSTS = new(lineSegmentsCount - 1, Allocator.Persistent);
        NativeArray<QST_Segment> RightFadeIn_QSTS = new(lineSegmentsCount - 1, Allocator.Persistent);
        transitionBuilder.BuildFadeOutTransition(qsLeft_, ref LeftFadeOut_QSTS);
        transitionBuilder.BuildFadeInTransition(qsRight, ref RightFadeIn_QSTS);
        QS_Transition fadeOutTransition = new QS_Transition(LeftFadeOut_QSTS);
        QS_Transition fadeInTransition = new QS_Transition(RightFadeIn_QSTS);

        var transBuffer = QS_Transition.PrepareConcatenationBuffer(fadeOutTransition, fadeInTransition, Allocator.Persistent);
        var transConc = QS_Transition.Concatenate(fadeOutTransition, fadeInTransition, transBuffer);
        MeshDataLineSegmets meshDataConc = new MeshDataLineSegmets(lineSegmentsCount * 2);
        NativeArray<QST_Segment> dataConc = new(lineSegmentsCount * 2 - 1, Allocator.Persistent);

        yield return new WaitForSeconds(1);
        GameObject.Destroy(meshRight.gameObject);
        QST_Animator animConc = new QST_Animator(meshDataConc.Vertices, meshDataConc.Indices, normalUV);

        animConc.AssignTransition(transConc);
        float lerpParam = 0;
        MeshBuffersIndexers biConc = new MeshBuffersIndexers();
        while (lerpParam < 1)
        {
            lerpParam += PlayModeTestsParams.FastLerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            float easedLerp = EaseOut(lerpParam);
            animConc.UpdateWithLerpPos(easedLerp, shouldReorientVertices: false, ref biConc);
            MeshGenUtils.ApplyMeshBuffers(meshDataConc.Vertices, meshDataConc.Indices, meshLeft_, biConc);
            biConc.Reset();
            yield return null;
        }

        meshDataLeft_.Dispose();
        meshDataRight.Dispose();
        meshDataConc.Dispose();
        dataConc.Dispose();
        transConc.DisposeConcatenation();
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
        GameObject.Destroy(meshLeft_.gameObject);
    }
}
