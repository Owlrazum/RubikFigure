using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using rnd = Unity.Mathematics.Random;

public abstract class FigureShuffleState : FigureState
{ 
    private const int FastSteps = 0;
    private const float FastSpeed = 10;
    private const float FastPauseTime = 0.1f;
    private const float FastShuffleTime = 1 / FastSpeed + FastPauseTime;

    protected float _shuffleLerpSpeed;

    private float2 _shuffleTimer;
    private int2 _shuffleStep;
    private int2 _dims;

    protected int2[][] _shuffleIndices;

    private bool _isShuffleCompleted;

    protected abstract void ShuffleIndices();
    protected virtual bool CustomShuffle(float lerpSpeed, out FigureVerticesMove[] moves)
    {
        moves = null;
        return false;
    }

    public FigureShuffleState(FigureStatesController statesController, Figure figure, FigureParamsSO figureParams)
    : base (statesController, figure)
    {
        _shuffleLerpSpeed = figureParams.ShuffleLerpSpeed;

        _shuffleTimer = new float2(0, 1 / _shuffleLerpSpeed + figureParams.ShufflePauseTime);
        _shuffleStep = new int2(0, figureParams.ShuffleStepsAmount);
        _dims = figureParams.FigureGenParamsSO.Dimensions;

        _shuffleIndices = new int2[_dims.y][];
        for (int row = 0; row < _dims.y; row++)
        {
            _shuffleIndices[row] = new int2[_dims.x];
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

    public override void OnEnter()
    {
        Debug.Log("OnEnter");
        _shuffleTimer.x = _shuffleTimer.y / 1.5f;
        _shuffleStep.x = 0;
    }

    public override void ProcessState()
    {
        _shuffleTimer.x += Time.deltaTime;
        if (_shuffleStep.x < FastSteps)
        {
            if (_shuffleTimer.x >= FastShuffleTime)
            {
                _isShuffleCompleted = false;
                Shuffle(FastSpeed);
                _shuffleTimer.x = 0;
                _shuffleStep.x++;
            }
        }
        else if (_shuffleStep.x < _shuffleStep.y)
        {
            if (_shuffleTimer.x >= _shuffleTimer.y)
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
            _figure.Shuffle(customMoves, ShuffleCompleteAction);
            return;
        }
        ShuffleIndices();

        FigureVerticesMove[] moves = new FigureVerticesMove[_dims.y * _dims.x];
        int moveIndex = 0;
        for (int ring = 0; ring < _shuffleIndices.Length; ring++)
        {
            for (int side = 0; side < _shuffleIndices[ring].Length; side++)
            {
                int2 fromIndex = new int2(side, ring);
                int2 toIndex = _shuffleIndices[ring][side];
                FigureVerticesMove shuffleMove = new FigureVerticesMove();
                shuffleMove.AssignFromIndex(fromIndex);
                shuffleMove.AssignToIndex(toIndex);
                shuffleMove.AssignLerpSpeed(lerpSpeed);
                shuffleMove.ShouldDisposeTransition = true;

                moves[moveIndex++] = shuffleMove;
            }
        }

        _figure.Shuffle(moves, ShuffleCompleteAction);
    }

    protected void ShuffleCompleteAction()
    {
        _isShuffleCompleted = true;
    }
}