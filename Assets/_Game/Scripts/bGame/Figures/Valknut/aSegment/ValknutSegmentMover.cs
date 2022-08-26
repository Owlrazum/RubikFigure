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

    private MeshBuffersIndexers _buffersData;
    private QSTransition _quadStripTransition;

    private ValknutSegmentMoveJob _moveJob;

    public override void Initialize(NativeArray<VertexData> verticesArg)
    {
        verticesArg.Dispose();

        _vertices = new NativeArray<VertexData>(
            (ValknutGenerator.MaxRangesCountForOneSegment + 2) * 2, Allocator.Persistent);
        _indices = new NativeArray<short>(ValknutGenerator.MaxRangesCountForOneSegment * 6, Allocator.Persistent);

        _buffersData = new MeshBuffersIndexers();
        _quadStripTransition = new QSTransition(ref _vertices, ref _indices);
        _moveJob = new ValknutSegmentMoveJob(ref _buffersData, ref _quadStripTransition);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        CollectionUtilities.DisposeIfNeeded(_vertices);
        CollectionUtilities.DisposeIfNeeded(_indices);
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
        Assert.IsTrue(verticesMove.TransitionData.IsCreated);
        _quadStripTransition.AssignTransitionData(
            verticesMove.TransitionData
        );
        // _moveJob.InputQuadStripTransition.AssignTransitionData(
        //     verticesMove.TransitionPositions,
        //     verticesMove.LerpRanges
        // );

        // _moveJob.P_LerpParam = EaseInOut(0.5f);
        // _segmentMoveJobHandle = _moveJob.Schedule(_segmentMoveJobHandle);
        // _wasJobScheduled = true;

        while (lerpParam < 1)
        {
            lerpParam += _currentLerpSpeed * Time.deltaTime;
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }

            _quadStripTransition.UpdateWithLerpPos(EaseInOut(lerpParam), ref _buffersData);
            AssignMeshBuffers(_vertices, _indices, _buffersData);
            _buffersData.Count = int2.zero;
            _buffersData.Start = int2.zero;
            _buffersData.LocalCount = int2.zero;
            print(EaseInOut(lerpParam));
            // _moveJob.P_LerpParam = EaseInOut(lerpParam);
            // _segmentMoveJobHandle = _moveJob.Schedule(_segmentMoveJobHandle);
            // _wasJobScheduled = true;
            yield return null;
        }

        _wasMoveCompleted = true;
    }

    private void LateUpdate()
    {
        if (_wasMoveCompleted)
        {
            // _segmentMoveJobHandle.Complete();
            AssignMeshBuffers(_vertices, _indices, _moveJob.BuffersData);

            _moveCompleteAction?.Invoke();
            _wasMoveCompleted = false;
        }
        else if (_wasJobScheduled)
        {
            _segmentMoveJobHandle.Complete();
            print($"{_moveJob.BuffersData} ; {_buffersData}");
            // MeshBuffersData buffersData = new MeshBuffersData();
            // buffersData.Count = new int2(10, 24);
            AssignMeshBuffers(_vertices, _indices, _buffersData);
            
            _wasJobScheduled = false;
        }
    }
}