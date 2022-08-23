using System.Collections.Generic;
using UnityEngine;

public class ValknutMoveState : FigureMoveState
{
    public ValknutMoveState(ValknutStatesController statesController, Valknut valknut, float moveLerpSpeed) :
        base(statesController, valknut, moveLerpSpeed)
    {
    }

    protected override List<FigureSegmentMove> DetermineMovesFromInput(Vector3 worldPos, Vector3 worldDir)
    {
        throw new System.NotImplementedException();
    }
}