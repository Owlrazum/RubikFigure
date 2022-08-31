using System.Collections.Generic;
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
        // Debug.Log(moves);
        // FigureSegmentMove[] move = new FigureSegmentMove[1];
        // move[0] = new FigureVerticesMove();
        // move[0].AssignFromIndex(new int2(1, 0));
        // move[0].AssignToIndex(new int2(2, 0));
        // move[0].AssignLerpSpeed(lerpSpeed);

        _figure.MakeMoves(moves, null);
        return null;
    }

    protected override void ShuffleIndices()
    {
        // HashSet<int2> toShuffle = new HashSet<int2>();
        // HashSet<int2> freeIndices = new HashSet<int2>();
        // for (int triangle = 0; triangle < _figure.ColCount; triangle++)
        // {
        //     for (int part = 0; part < _figure.RowCount; part++)
        //     {
        //         int2 index = new int2(triangle, part);
        //         toShuffle.Add(index);
        //         freeIndices.Add(index);
        //     }
        // }

        for (int triangle = 0; triangle < _figure.ColCount; triangle++)
        {
            int nextTriangle = triangle + 1 >= _figure.ColCount ? 0 : triangle + 1;
            if (rnd.value < 0.5f)
            {
                if (rnd.value < 0.5f)
                { 
                    _shuffleIndices[0][triangle] = new int2(nextTriangle, 0);
                    _shuffleIndices[1][triangle] = new int2(nextTriangle, 1);
                }
                else
                { 
                    _shuffleIndices[0][triangle] = new int2(nextTriangle, 1);
                    _shuffleIndices[1][triangle] = new int2(nextTriangle, 0);
                }
            }
        }
    }

    private void ConvertToVerticesMoves(FigureSegmentMove[] moves)
    { 
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = new FigureVerticesMove(moves[i]);
            Assert.IsNotNull(moves[i]);
        }
    }
}