using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using rnd = UnityEngine.Random;

using Orazum.Collections;

public class ValknutShuffleState : FigureShuffleState
{
    public ValknutShuffleState(ValknutStatesController statesController, Valknut valknut, FigureParamsSO figureParams)
        : base(statesController, valknut, figureParams) 
    {
    }

    protected override FigureSegmentMove[] Shuffle(float lerpSpeed)
    {
        FigureSegmentMove[] moves = base.Shuffle(lerpSpeed);
        ConvertToVerticesMoves(moves);
        Debug.Log(moves);
        // FigureSegmentMove[] move = new FigureSegmentMove[1];
        // move[0] = new ValknutVerticesMove();
        // move[0].AssignFromIndex(new int2(0, 1));
        // move[0].AssignToIndex(new int2(1, 0));
        // move[0].AssignLerpSpeed(lerpSpeed);

        _figure.MakeMoves(moves, null);
        return null;
    }

    protected override void ShuffleIndices()
    {
        for (int part = 0; part < _figure.RowCount; part++)
        {
            for (int triangle = 0; triangle < _figure.ColCount; triangle++)
            {
                _shuffleIndices[part][triangle] = new int2(triangle, part);
            }
        }

        for (int part = 0; part < _figure.RowCount; part++)
        {
            _shuffleIndices[part] = Algorithms.RandomDerangement(in _shuffleIndices[part]);
        }

        for (int triangle = 0; triangle < _figure.ColCount; triangle++)
        {
            if (rnd.value > 0.5f)
            {
                int2 t = _shuffleIndices[0][triangle];
                _shuffleIndices[0][triangle] = _shuffleIndices[1][triangle];
                _shuffleIndices[1][triangle] = t;
            }
        }
    }

    private void ConvertToVerticesMoves(FigureSegmentMove[] moves)
    { 
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = new ValknutVerticesMove(moves[i]);
            Assert.IsNotNull(moves[i]);
        }
    }
}