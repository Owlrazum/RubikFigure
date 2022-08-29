using System;
using System.Collections;

using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Meshing;
using Orazum.Collections;
using static Orazum.Math.MathUtilities;

[RequireComponent(typeof(MeshFilter))]
public class ValknutSegmentMover : FigureSegmentMover
{ 
    private const float ClockMoveBufferLerpValue = 0.4f;

    private bool _wasJobScheduled;
    private bool _wasMoveCompleted;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;
    private NativeArray<MeshBuffersIndexers> _indexersShared;
    
    private QSTransition _quadStripTransition;

    private float3x2 _normalUV;

    public override void Initialize(NativeArray<VertexData> verticesArg)
    {
        _normalUV = new float3x2(verticesArg[0].normal, new float3(verticesArg[0].uv, 0));
        verticesArg.Dispose();

        _vertices = new NativeArray<VertexData>(
            (ValknutGenerator.MaxRangesCountForOneSegment + 2) * 2, Allocator.Persistent);
        _indices = new NativeArray<short>(ValknutGenerator.MaxRangesCountForOneSegment * 6, Allocator.Persistent);
        _indexersShared = new NativeArray<MeshBuffersIndexers>(1, Allocator.Persistent);
        
        _quadStripTransition = new QSTransition(ref _vertices, ref _indices);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        CollectionUtilities.DisposeIfNeeded(_vertices);
        CollectionUtilities.DisposeIfNeeded(_indices);
        CollectionUtilities.DisposeIfNeeded(_indexersShared);
    }

    public override void StartMove(
        FigureSegmentMove move,
        Action OnMoveToDestinationCompleted)
    {
        base.StartMove(move, OnMoveToDestinationCompleted);
        _wasMoveCompleted = false;

        if (move is ValknutVerticesMove verticesMove)
        {
            StartCoroutine(MoveSequence(verticesMove));
        }
    }

    private IEnumerator MoveSequence(ValknutVerticesMove verticesMove)
    {
        float lerpParam = 0;
        Assert.IsTrue(verticesMove.TransSegments.IsCreated);
        // _quadStripTransition.AssignTransitionData(
        //     verticesMove.TransSegments,
        //     _normalUV
        // );
        _quadStripTransition.AssignTransitionData(verticesMove.TransSegments, _normalUV);
        ValknutSegmentMoveJob moveJob = new ValknutSegmentMoveJob()
        {
            InputQuadStripTransition = _quadStripTransition,
            OutputIndexers = _indexersShared
        };

        while (lerpParam < 1)
        {
            lerpParam += _currentLerpSpeed * Time.deltaTime;
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }

            // _quadStripTransition.UpdateWithLerpPos(EaseInOut(lerpParam), ref _buffersData);
            // AssignMeshBuffers(_vertices, _indices, _buffersData);
            // print(EaseInOut(lerpParam));
            moveJob.P_LerpParam = EaseInOut(lerpParam);
            _segmentMoveJobHandle = moveJob.Schedule(_segmentMoveJobHandle);
            _wasJobScheduled = true;
            yield return null;
        }

        _wasMoveCompleted = true;
    }

    private void LateUpdate()
    {
        if (_wasMoveCompleted)
        {
            MeshBuffersIndexers indexers = _indexersShared[0];
            indexers.Reset();
            _indexersShared[0] = indexers;

            _moveCompleteAction?.Invoke();
            _wasMoveCompleted = false;
        }
        else if (_wasJobScheduled)
        {
            _segmentMoveJobHandle.Complete();
            MeshBuffersIndexers indexers = _indexersShared[0];
            AssignMeshBuffers(_vertices, _indices, indexers);
            indexers.Reset();
            _indexersShared[0] = indexers;
            
            _wasJobScheduled = false;
        }
    }
}