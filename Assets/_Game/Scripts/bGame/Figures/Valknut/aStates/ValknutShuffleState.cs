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

    // protected override void Shuffle(float lerpSpeed)
    // {
    //     FigureSegmentMove[] move = new FigureSegmentMove[1];
    //     move[0] = new FigureVerticesMove();
    //     move[0].AssignFromIndex(new int2(1, 0));
    //     move[0].AssignToIndex(new int2(2, 0));
    //     move[0].AssignLerpSpeed(lerpSpeed);

    //     _figure.MakeMoves(move, ShuffleCompleteAction);
    // }

    protected override void ShuffleIndices()
    {
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
}