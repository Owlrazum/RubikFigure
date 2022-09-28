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
using static Orazum.Constants.Math;
using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Math.EasingUtilities;
using static Orazum.Math.MathUtils;

public class ValknutTransitionsTests : TransitionsTester
{
    private MeshDataLineSegments _originMesh;
    private MeshDataLineSegments _targetMesh;

    protected override ref MeshDataLineSegments FirstStartMeshData => ref _originMesh;
    protected override ref MeshDataLineSegments SecondStartMeshData => ref _targetMesh;

    private LineEndDirectionType _originDirection;
    private LineEndDirectionType _targetDirection;

    private LineEndType _targetRayQuadStripEnd;
    private LineEndDirectionType _targetRayDirection;
    private LineEndType _targetRayLineSegmentSide;

    [UnityTest]
    public IEnumerator OriginTargetDirections()
    {
        yield return SESE(isFirst: true);
        yield return ESSE();
    }

    private IEnumerator ESSE(bool isFirst = false)
    {
        _originDirection = LineEndDirectionType.EndToStart;
        _targetDirection = LineEndDirectionType.StartToEnd;

        _targetRayQuadStripEnd = LineEndType.Start;
        _targetRayDirection = LineEndDirectionType.EndToStart;
        _targetRayLineSegmentSide = LineEndType.End;

        MeshGenUtils.SquareArcMesh(
            center: new float3(-0.5f, 0, 1f),
            angle: -TAU / 4,
            width: 1,
            gapWidth: 0.3f,
            gapHeight: 2,
            out _originMesh
        );
        MeshGenUtils.SquareArcMesh(
            center: new float3(1.5f, 0, -1),
            angle: 0,
            width: 1,
            gapWidth: 0.5f,
            gapHeight: 2f,
            out _targetMesh
        );
        yield return TestCase(isFirst);
    }

    private IEnumerator SESE(bool isFirst)
    { 
        _originDirection = LineEndDirectionType.StartToEnd;
        _targetDirection = LineEndDirectionType.StartToEnd;

        _targetRayQuadStripEnd = LineEndType.Start;
        _targetRayDirection = LineEndDirectionType.EndToStart;
        _targetRayLineSegmentSide = LineEndType.End;
        MeshGenUtils.SquareArcMesh(
            center: new float3(-0.5f, 0, -1.3f),
            angle: -TAU / 4,
            width: 1,
            gapWidth: 0.3f,
            gapHeight: 2,
            out _originMesh
        );
        MeshGenUtils.SquareArcMesh(
            center: new float3(1.5f, 0, -1),
            angle: 0,
            width: 1,
            gapWidth: 0.5f,
            gapHeight: 2f,
            out _targetMesh
        );
        yield return TestCase(isFirst);
    }

    private IEnumerator TestCase(bool shouldInitializeScene = false)
    {
        if (shouldInitializeScene)
        {
            yield return InitializeScene();
            yield return new WaitForSeconds(1);
        }
        DestroyMeshDummies();
        yield return TestTransition(Initializer);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
    }

    private delegate void RayInitializerDelegate(ref ValknutTransitionsBuilder builder);
    private IEnumerator TestTransition(RayInitializerDelegate Initializer)
    {
        QuadStrip originQs = new QuadStrip(_originMesh.LineSegments);
        QuadStrip targetQs = new QuadStrip(_targetMesh.LineSegments);
        ValknutTransitionsBuilder builder = new(originQs, targetQs);
        NativeArray<QST_Segment> writeBuffer = new NativeArray<QST_Segment>(originQs.QuadsCount + 1 + targetQs.QuadsCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        Initializer.Invoke(ref builder);
        bool isSuccess = builder.BuildTransition(_originDirection, _targetDirection, ref writeBuffer);
        if (!isSuccess)
        {
            Debug.LogError("Transition build failed!");
            _originMesh.Dispose();
            _targetMesh.Dispose();

            writeBuffer.Dispose();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
            yield break;
        }

        QS_Transition transition = new QS_Transition(writeBuffer);
        _transitionMesh = new(writeBuffer.Length * 2);
        _animator = new(_transitionMesh.Vertices, _transitionMesh.Indices, NormalUV);
        _animator.AssignTransition(transition);

        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter mesh);
        yield return TestTransition(mesh, PlayModeTestsParams.ExtraSlowLerpSpeed);
        _originMesh.Dispose();
        _targetMesh.Dispose();

        writeBuffer.Dispose(); ;
        _transitionMesh.Dispose();
        GameObject.Destroy(mesh.gameObject);
    }

    private IEnumerator TestSegments(NativeArray<QST_Segment> writeBuffer)
    {
        for (int i = 0; i < writeBuffer.Length; i++)
        {
            DrawLineSegmentWithRaysUp(writeBuffer[i].StartLineSegment, 1, 3);
            yield return new WaitForSeconds(1);
            DrawLineSegmentWithRaysUp(writeBuffer[i].EndLineSegment, 1, 2);
            yield return new WaitForSeconds(2);
        }
    }

    private void Initializer(ref ValknutTransitionsBuilder builder)
    {
        builder.InitializeTargetRay(_targetRayQuadStripEnd, _targetRayDirection, _targetRayLineSegmentSide);
    }
}
