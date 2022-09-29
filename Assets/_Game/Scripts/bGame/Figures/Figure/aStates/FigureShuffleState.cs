using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

using rnd = UnityEngine.Random;

public class FigureShuffleState : FigureState
{ 
    private const int FastSteps = 0;
    private const float FastSpeed = 10;
    private const float FastPauseTime = 0.1f;
    private const float FastShuffleTime = 1 / FastSpeed + FastPauseTime;

    protected float _shuffleLerpSpeed;

    private float2 _shuffleTimer;
    private int2 _shuffleStep;
    private int2 _dims;

    protected Array2D<int2> _shuffleIndices;

    private bool _isShuffleCompleted;

    protected virtual bool CustomShuffle(float lerpSpeed, out FSMCT_Shuffle[] moves)
    {
        moves = null;
        return false;
    }

    public FigureShuffleState(FigureStatesController statesController, Figure figure, FigureParamsSO figureParams, FigureGenParamsSO genParams)
    : base (statesController, figure)
    {
        _shuffleLerpSpeed = figureParams.ShuffleLerpSpeed;

        _shuffleTimer = new float2(0, 1 / _shuffleLerpSpeed + figureParams.ShufflePauseTime);
        _shuffleStep = new int2(0, figureParams.ShuffleStepsAmount);
        _dims = genParams.Dimensions;

        _shuffleIndices = new Array2D<int2>(_dims);
        for (int row = 0; row < _dims.y; row++)
        {
            for (int col = 0; col < _dims.x; col++)
            {
                _shuffleIndices[col, row] = new int2(-1, -1);
            }
        }
    }

    public override FigureState HandleTransitions()
    {
        if (_shuffleStep.x >= _shuffleStep.y && _isShuffleCompleted)
        {
            return _statesController.IdleState;
        }
        
        return null;
    }

     protected void ShuffleCompleteAction()
    {
        _isShuffleCompleted = true;
    }

    public override void OnEnter()
    {
        _shuffleTimer.x = _shuffleTimer.y / 1.5f;
        _shuffleStep.x = 0;
        _isShuffleCompleted = true;
    }

    public override void ProcessState()
    {
        _shuffleTimer.x += Time.deltaTime;
        if (_shuffleStep.x < FastSteps)
        {
            if (_shuffleTimer.x >= FastShuffleTime && _isShuffleCompleted)
            {
                _isShuffleCompleted = false;
                Shuffle(FastSpeed);
                _shuffleTimer.x = 0;
                _shuffleStep.x++;
            }
        }
        else if (_shuffleStep.x < _shuffleStep.y)
        {
            if (_shuffleTimer.x >= _shuffleTimer.y && _isShuffleCompleted)
            {
                _isShuffleCompleted = false;
                Shuffle(_shuffleLerpSpeed);
                _shuffleTimer.x = 0;
               _shuffleStep.x++;
            }
        }
    }

    protected virtual void Shuffle(float lerpSpeed)
    {
        if (CustomShuffle(lerpSpeed, out var customMoves))
        {
            _figure.MakeMoves(customMoves, ShuffleCompleteAction);
            return;
        }
        ShuffleIndices();

        FSMCT_Shuffle[] moves = new FSMCT_Shuffle[_dims.y * _dims.x];
        int moveIndex = 0;
        for (int row = 0; row < _shuffleIndices.RowCount; row++)
        {
            for (int col = 0; col < _shuffleIndices.ColCount; col++)
            {
                int2 fromIndex = new int2(col, row);
                int2 toIndex = _shuffleIndices[fromIndex];
                FSMCT_Shuffle shuffleMove = new FSMCT_Shuffle();
                shuffleMove.AssignFromIndex(fromIndex);
                shuffleMove.AssignToIndex(toIndex);
                shuffleMove.AssignLerpSpeed(lerpSpeed);
                moves[moveIndex++] = shuffleMove;
            }
        }

        ResetShuffleIndices();
        _figure.MakeMoves(moves, ShuffleCompleteAction);
    }

    protected virtual void ShuffleIndices()
    {
        List<int2> avalableIndices = new List<int2>(_shuffleIndices.ColCount * _shuffleIndices.RowCount);
        int2 index = new int2(0, 0);
        for (int i = 0; i < avalableIndices.Capacity; i++)
        {
            avalableIndices.Add(index);
            MoveIndex(ref index);
        }

        index = int2.zero;
        for (int i = 0; i < avalableIndices.Capacity; i++)
        {
            int rndIndex = rnd.Range(0, avalableIndices.Count);
            _shuffleIndices[index] = avalableIndices[rndIndex];
            avalableIndices.RemoveAt(rndIndex);
            MoveIndex(ref index);
        }
    }

    protected void MoveIndex(ref int2 index)
    {
        if (index.x + 1 >= _shuffleIndices.ColCount)
        {
            index.x = 0;
            index.y++;
        }
        else
        {
            index.x++;
        }
    }

    private void ResetShuffleIndices()
    {
        for (int row = 0; row < _figure.RowCount; row++)
        {
            for (int col = 0; col < _figure.ColCount; col++)
            {
                _shuffleIndices[col, row] = new int2(-1, -1);
            }
        }
    }
}