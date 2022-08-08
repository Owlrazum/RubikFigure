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

    private Wheel _currentWheel;
    private float _currentShuffleTimer;
    private int _currentStep;

    public ShuffleState(WheelGenerationData generationData) : base(generationData)
    {
        _shuffleLerpSpeed = generationData.LevelDescriptionSO.ShuffleLerpSpeed;
        _shufflePauseTime = generationData.LevelDescriptionSO.ShufflePauseTime;

        _shuffleStepsAmount = generationData.LevelDescriptionSO.ShuffleStepsAmount;
        _fastShuffleTime = 1 / FAST_SPEED + _shufflePauseTime / 10;
        _shuffleTime = 1 / _shuffleLerpSpeed + _shufflePauseTime;

        _randomGenerator = rnd.CreateFromIndex(15);

        _possibleMoves = new List<SegmentMove>(4);

        Subscribe();
    }

    public override WheelState HandleTransitions()
    {
        if (_currentStep >= _shuffleStepsAmount)
        {
            return WheelStatesDelegates.IdleState();
        }
        
        return null;
    }

    public override void OnEnter()
    {
        _currentStep = 0;
    }

    public override void StartProcessingState(Wheel wheel)
    {
        _currentWheel = wheel;
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
        int2[] emptyIndices = _currentWheel.GetEmptyIndices();
        for (int i = 0; i < emptyIndices.Length; i++)
        {
            int2 emptyIndex = emptyIndices[i];

            DeterminePossibleMoves(emptyIndex);
            if (_possibleMoves.Count == 0)
            {
                continue;
            }

            int rnd = _randomGenerator.NextInt(0, _possibleMoves.Count);
            SegmentMove randomMove = _possibleMoves[rnd];
            randomMove.IsValid = true;
            randomMove.EmtpyPointIndex = i;

            _currentWheel.MakeMove(in randomMove, lerpSpeed);

            _possibleMoves.Clear();
        }
    }

    private void DeterminePossibleMoves(int2 emptyIndex)
    {
        if (_currentWheel.HasSegmentThatWillMoveDown(emptyIndex))
        {
            _possibleMoves.Add(new SegmentMove(SegmentMoveType.Down, _currentWheel.MoveIndexUp(emptyIndex), emptyIndex));
        }
        if (_currentWheel.HasSegmentThatWillMoveUp(emptyIndex))
        {
            _possibleMoves.Add(new SegmentMove(SegmentMoveType.Up, _currentWheel.MoveIndexDown(emptyIndex), emptyIndex));
        }
        if (_currentWheel.HasSegmentThatWillMoveCounterClockwise(emptyIndex))
        {
            _possibleMoves.Add(
                new SegmentMove(SegmentMoveType.CounterClockwise, 
                _currentWheel.MoveIndexClockwise(emptyIndex), emptyIndex)
            );
        }
        if (_currentWheel.HasSegmentThatWillMoveClockwise(emptyIndex))
        {
            _possibleMoves.Add(
                new SegmentMove(SegmentMoveType.Clockwise, 
                _currentWheel.MoveIndexCounterClockwise(emptyIndex), emptyIndex));
        }
    }

    private void Subscribe()
    {
        WheelStatesDelegates.ShuffleState += GetThisState;
    }

    public override void OnDestroy()
    {
        WheelStatesDelegates.ShuffleState -= GetThisState;
    }

    protected override WheelState GetThisState()
    {
        return this;
    }
}