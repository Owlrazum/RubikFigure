using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

public class WheelShuffleState : FigureShuffleState
{
    public WheelShuffleState(WheelStatesController statesController, Wheel wheel, FigureParamsSO figureParams)
    : base (statesController, wheel, figureParams)
    {
    }

    private bool isCustomShuffled;
    protected override bool CustomShuffle(float lerpSpeed, out FigureVerticesMove[] customMoves)
    {
        if (isCustomShuffled)
        {
            return base.CustomShuffle(lerpSpeed, out customMoves);
        }
        customMoves = new FigureVerticesMove[1];
        var move = new FigureVerticesMove();
        move.AssignFromIndex(new int2(0,1));
        move.AssignToIndex(new int2(1,1));
        move.AssignLerpSpeed(lerpSpeed);
        move.ShouldDisposeTransition = true;
        customMoves[0] = move;
        isCustomShuffled = true;
        return true;
    }

    protected override void ShuffleIndices()
    {
        for (int ring = 0; ring < _figure.RowCount; ring++)
        {
            for (int side = 0; side < _figure.ColCount; side++)
            {
                _shuffleIndices[ring][side] = new int2(side, ring);
            }
        }

        for (int ring = 0; ring < _figure.RowCount; ring++)
        {
            _shuffleIndices[ring] = Algorithms.RandomDerangement(in _shuffleIndices[ring]);
        }
    }
}