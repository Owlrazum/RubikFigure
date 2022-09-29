using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

using Orazum.Meshing;
using static Orazum.Math.MathUtils;
using static Orazum.Math.EasingUtilities;

public abstract class TransitionsTester
{
    protected virtual float3 CameraPos => new float3(0, 13, 0);
    protected virtual float3 CameraForward => math.down();
    protected virtual float3 CameraUp => math.forward();
    
    protected virtual float3 LightForward => new float3(0, -1, 1);
    protected virtual float3 LightUp => math.forward();

    protected virtual float3x2 NormalUV => new float3x2(new float3(0, 1, 0), float3.zero);

    protected abstract ref MeshDataLineSegments FirstStartMeshData { get; }
    protected abstract ref MeshDataLineSegments SecondStartMeshData { get; }

    protected QST_Animator _animator;
    protected MeshData _transitionMesh;

    private List<MeshFilter> _meshDummies;
    protected void DestroyMeshDummies()
    {
        foreach (MeshFilter m in _meshDummies)
        { 
            GameObject.Destroy(m.gameObject);
        }
        _meshDummies.Clear();
    }

    protected IEnumerator InitializeScene()
    { 
        PlayModeTestsUtils.CreateCamera(CameraPos, CameraForward, CameraUp);
        PlayModeTestsUtils.CreateLight(LightForward, LightUp);

        _meshDummies = new List<MeshFilter>(2);
        CreateMesh(FirstStartMeshData);
        CreateMesh(SecondStartMeshData);
        yield return null;
    }

    private void CreateMesh(in MeshDataLineSegments meshData)
    { 
        QuadStripBuilder builder = new QuadStripBuilder(meshData.Vertices, meshData.Indices, NormalUV);
        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        builder.Build(meshData.LineSegments, ref buffersIndexers);
        PlayModeTestsUtils.CreateMeshDummy(out MeshFilter mesh);
        MeshGenUtils.ApplyMeshBuffers(meshData.Vertices, meshData.Indices, mesh, buffersIndexers);
        _meshDummies.Add(mesh);
    }

    protected IEnumerator TestTransition(MeshFilter mesh, float lerpSpeed)
    { 
        float lerpParam = 0;
        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        while (lerpParam < 1)
        {
            lerpParam += lerpSpeed * Time.deltaTime;
            ClampToOne(ref lerpParam);
            _animator.UpdateWithLerpPos(EaseOut(lerpParam), ref buffersIndexers);
            MeshGenUtils.ApplyMeshBuffers(_transitionMesh.Vertices, _transitionMesh.Indices, mesh, buffersIndexers);
            buffersIndexers.Reset();
            yield return null;
        }
    }
}