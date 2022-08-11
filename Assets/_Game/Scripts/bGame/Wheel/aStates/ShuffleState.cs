using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;

using rnd = Unity.Mathematics.Random;

public class ShuffleState : WheelState
{ 
    private const int FAST_STEPS = 0;
    private const float FAST_SPEED = 10;

    private float _shuffleLerpSpeed;
    private float _shufflePauseTime;

    private int _shuffleStepsAmount;

    private rnd _randomGenerator;
    private List<SegmentMove> _possibleMoves;

    private float _fastShuffleTime;
    private float _shuffleTime;

    private float _currentShuffleTimer;
    private int _currentStep;

    private int2[] _currentEmptyIndices;

    public ShuffleState(LevelDescriptionSO levelDescription, Wheel wheelArg) : base(levelDescription, wheelArg)
    {
        _shuffleLerpSpeed = levelDescription.ShuffleLerpSpeed;
        _shufflePauseTime = levelDescription.ShufflePauseTime;

        _shuffleStepsAmount = levelDescription.ShuffleStepsAmount;
        _fastShuffleTime = 1 / FAST_SPEED + _shufflePauseTime / 10;
        _shuffleTime = 1 / _shuffleLerpSpeed + _shufflePauseTime;

        _randomGenerator = rnd.CreateFromIndex(100203);

        _possibleMoves = new List<SegmentMove>(4);

        Subscribe();
    }

    public void PrepareForShuffle(int2[] currentEmptyIndicesArg)
    {
        _currentEmptyIndices = currentEmptyIndicesArg;
    }

    public override WheelState HandleTransitions()
    {
        if (_currentStep >= _shuffleStepsAmount)
        {
            return WheelDelegates.IdleState();
        }
        
        return null;
    }

    public override void OnEnter()
    {
        _currentStep = 0;
    }

    public override void ProcessState()
    {
        _currentShuffleTimer += Time.deltaTime;
        // Debug.Log("Processing shuffleState " + _currentShuffleTimer);
        if (_currentStep < FAST_STEPS)
        {
            if (_currentShuffleTimer >= _fastShuffleTime)
            {
                Shuffle(FAST_SPEED);
                _currentShuffleTimer = 0;
                _currentStep++;
            }
        }
        else
        {
            if (_currentShuffleTimer >= _shuffleTime)
            {
                Shuffle(_shuffleLerpSpeed);
                _currentShuffleTimer = 0;
               _currentStep++;
            }
        }
    }

    private void Shuffle(float lerpSpeed)
    {
        for (int i = 0; i < _currentEmptyIndices.Length; i++)
        {
            int2 emptyIndex = _currentEmptyIndices[i];

            _wheel.DeterminePossibleMoves(emptyIndex, _possibleMoves);
            if (_possibleMoves.Count == 0)
            {
                continue;
            }

            int rnd = _randomGenerator.NextInt(0, _possibleMoves.Count);
            SegmentMove randomMove = _possibleMoves[rnd];
            _currentEmptyIndices[i] = randomMove.FromIndex;

            _wheel.MakeMove(in randomMove, lerpSpeed);
            _possibleMoves.Clear();
        }
    }

    private void Subscribe()
    {
        WheelDelegates.ShuffleState += GetThisState;
    }

    public override void OnDestroy()
    {
        WheelDelegates.ShuffleState -= GetThisState;
    }

    protected override WheelState GetThisState()
    {
        return this;
    }
}