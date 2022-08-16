using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;

using Orazum.Collections;
using static Orazum.Math.MathUtilities;

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

        RotationMove[] moves = new RotationMove[_wheel.SideCount * _wheel.RingCount];
        int moveIndex = 0;
        for (int ring = 0; ring < _shuffleIndices.Length; ring++)
        {
            for (int side = 0; side < _shuffleIndices[ring].Length; side++)
            {
                int2 fromIndex = new int2(side, ring);
                int2 toIndex = _shuffleIndices[ring][side];
                RotationMove rotationMove = new RotationMove();
                rotationMove.AssignFromIndex(fromIndex);
                rotationMove.AssignToIndex(toIndex);
                rotationMove.AssignType(RotationMove.TypeType.Clockwise);

                float rotationAngle = 0;
                int sideDeltaCW = Mathf.Abs(toIndex.x - fromIndex.x);
                int sideDeltaCCW = Mathf.Abs(toIndex.x + _wheel.SideCount - fromIndex.x);
                if (sideDeltaCW < sideDeltaCCW)
                { 
                    rotationAngle = sideDeltaCW * TAU / _wheel.SideCount * Mathf.Rad2Deg;
                    if (toIndex.x < fromIndex.x)
                    {
                        rotationAngle = -rotationAngle;
                        rotationMove.AssignType(RotationMove.TypeType.CounterClockwise);
                    }
                }
                else
                { 
                    rotationAngle = sideDeltaCCW * TAU / _wheel.SideCount * Mathf.Rad2Deg;
                    if (toIndex.x + _wheel.SideCount < fromIndex.x)
                    {
                        rotationAngle = -rotationAngle;
                        rotationMove.AssignType(RotationMove.TypeType.CounterClockwise);
                    }
                }
                Debug.Log($"rotation angel {rotationAngle}");
                rotationMove.AssignRotation(Quaternion.AngleAxis(rotationAngle, Vector3.up));
                moves[moveIndex++] = rotationMove;
            }
        }
        _wheel.MakeShuffleMoves(moves, _shuffleLerpSpeed);
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
            _shuffleIndices[ring] = Algorithms.RandomDerangement(in _shuffleIndices[ring]);
            string log = "";
            for (int side = 0; side < _wheel.SideCount; side++)
            {
                log += _shuffleIndices[ring][side];
            }
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