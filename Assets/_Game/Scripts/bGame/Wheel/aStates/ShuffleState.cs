using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;

using Orazum.Collections;

using rnd = Unity.Mathematics.Random;

public class ShuffleState : WheelState
{ 
    private const int FAST_STEPS = 0;
    private const float FAST_SPEED = 10;
    private float _fastShuffleTime;

    private float _shuffleLerpSpeed;
    private float _shuffleTime;
    private float _shufflePauseTime;

    private int _shuffleStepsAmount;

    private rnd _randomGenerator;
    private int2[][] _shuffleIndices;

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

        _randomGenerator = rnd.CreateFromIndex((uint)System.DateTime.Now.Millisecond);

        _shuffleIndices = new int2[_wheel.RingCount][];
        for (int ring = 0; ring < _wheel.RingCount; ring++)
        {
            _shuffleIndices[ring] = new int2[_wheel.SideCount];
        }

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
        _currentShuffleTimer = _shuffleTime;
    }

    public override void ProcessState()
    {
        _currentShuffleTimer += Time.deltaTime;
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
        ShuffleIndices();

        SegmentMove[] moves = new SegmentMove[_wheel.SideCount * _wheel.RingCount];
        int moveIndex = 0;
        for (int ring = 0; ring < _shuffleIndices.Length; ring++)
        {
            for (int side = 0; side < _shuffleIndices[ring].Length; side++)
            {
                int2 fromIndex = new int2(side, ring);
                int2 toIndex = _shuffleIndices[ring][side];
                SegmentMove move = new SegmentMove(SegmentMoveType.Down, fromIndex, toIndex);
                moves[moveIndex++] = move;
            }
        }
        _wheel.MakeMoveCollection(moves, _shuffleLerpSpeed);
    }

    private void ShuffleIndices()
    {
        for (int ring = 0; ring < _wheel.RingCount; ring++)
        {
            for (int side = 0; side < _wheel.SideCount; side++)
            {
                _shuffleIndices[ring][side] = new int2(side, ring);
            }
        }

        for (int ring = 0; ring < _wheel.RingCount; ring++)
        {
            _shuffleIndices[ring] = Algorithms.RandomDerangement(_shuffleIndices[ring]);
            string log = "";
            for (int side = 0; side < _wheel.SideCount; side++)
            {
                log += _shuffleIndices[ring][side];
            }
            Debug.Log(log);
        }

    }

    private void ShuffleIndexTriangle(int3 indexTriangle)
    {
        // int3 element = _shuffleIndices[indexTriangle.x];

    }

    private void ShuffleIncrementally(float lerpSpeed)
    { 
        for (int i = 0; i < _currentEmptyIndices.Length; i++)
        {
            int2 emptyIndex = _currentEmptyIndices[i];

            List<SegmentMove> possibleMoves = new List<SegmentMove>();
            _wheel.DeterminePossibleMoves(emptyIndex, possibleMoves);
            if (possibleMoves.Count == 0)
            {
                continue;
            }

            int rnd = _randomGenerator.NextInt(0, possibleMoves.Count);
            SegmentMove randomMove = possibleMoves[rnd];
            _currentEmptyIndices[i] = randomMove.FromIndex;

            _wheel.MakeMove(in randomMove, lerpSpeed);
            possibleMoves.Clear();
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