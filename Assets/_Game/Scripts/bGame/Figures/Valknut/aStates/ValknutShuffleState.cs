using Unity.Mathematics;

using UnityEngine;

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
        return base.Shuffle(lerpSpeed);
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
}