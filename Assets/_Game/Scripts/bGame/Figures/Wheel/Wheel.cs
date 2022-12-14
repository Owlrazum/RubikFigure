using System;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Constants.Math;

public class Wheel : Figure
{
    public int SideCount { get { return _dims.x; } }
    public int RingCount { get { return _dims.y; } }

    private Array2D<WheelSegmentTransitions> _transitions;

    public void AssignTransitionDatas(in Array2D<WheelSegmentTransitions> transitions)
    {
        _transitions = transitions;
    }

    public override void Initialize(
        Array2D<FS_Point> segmentPoints,
        FigureParamsSO figureParams,
        FigureGenParamsSO genParams)
    {
        base.Initialize(segmentPoints, figureParams, genParams);
        RotateSegmentPointsOnGeneration();
    }
    private void RotateSegmentPointsOnGeneration()
    {
        for (int side = 0; side < _dims.x; side++)
        {
            for (int ring = 0; ring < _dims.y; ring++)
            {
                Quaternion rotation = GetSideRotation(side);
                int2 index = new int2(side, ring);
                _segmentPoints[index].transform.localRotation = rotation;
            }
        }
    }

    private Quaternion GetSideRotation(int2 index)
    {
        return GetSideRotation(index.x);
    }
    private Quaternion GetSideRotation(int sideIndex)
    {
        float rotationAngle = sideIndex * TAU / SideCount * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(rotationAngle, Vector3.up);
    }

    protected override void MakeSegmentMove(FigureSegment segment, FMSC_Transition move, Action moveCompleteAction)
    {
        AssignTransData(move);
        segment.StartMove(move, moveCompleteAction);
    }

    private void AssignTransData(FMSC_Transition move)
    {
        int2 from = move.From;
        int2 to = move.To;
        Assert.IsTrue(from.x == to.x || from.y == to.y);
        int sideDelta = to.x - from.x;
        int ringDelta = to.y - from.y;

        if (sideDelta > 0)
        {
            if (sideDelta == _dims.x - 1)
            { 
                move.Transition = _transitions[to].AntiCW;    
            }
            else
            { 
                move.Transition = _transitions[to].CW;
            }
        }

        if (sideDelta < 0)
        {
            if (sideDelta == -(_dims.x - 1))
            {
                move.Transition = _transitions[to].CW;
            }
            else
            {
                move.Transition = _transitions[to].AntiCW;
            }
        }

        if (ringDelta > 0)
        {
            if (ringDelta == _dims.y - 1)
            { 
                move.Transition = _transitions[to].Down;
            }
            else
            { 
                move.Transition = _transitions[to].Up;
            }
        }

        if (ringDelta < 0)
        {
            if (ringDelta == -(_dims.y - 1))
            {
                move.Transition = _transitions[to].Up;
            }
            else
            { 
                move.Transition = _transitions[to].Down;
            }
        }
    }

    [ContextMenu("Print")]
    public void Print()
    {
        Debug.Log(ToString());
    }

    protected override string GetFigureName()
    {
        return "Wheel";
    }
}
