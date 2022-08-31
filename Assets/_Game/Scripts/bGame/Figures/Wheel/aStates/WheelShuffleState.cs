using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Math.MathUtils;

public class WheelShuffleState : FigureShuffleState
{
    public WheelShuffleState(WheelStatesController statesController, Wheel wheel, FigureParamsSO figureParams)
    : base (statesController, wheel, figureParams)
    {
    }

    protected override FigureSegmentMove[] Shuffle(float lerpSpeed)
    {
        FigureSegmentMove[] moves = base.Shuffle(lerpSpeed);
        ConvertToVerticesMove(moves);
        Debug.Log(moves);
        _figure.MakeMoves(moves, null);
        return null;
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

    private void ConvertToVerticesMove(FigureSegmentMove[] moves)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = new FigureVerticesMove(moves[i]);
            Assert.IsNotNull(moves[i]);
        }
    }
}