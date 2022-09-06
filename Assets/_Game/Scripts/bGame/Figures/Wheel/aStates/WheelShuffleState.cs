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