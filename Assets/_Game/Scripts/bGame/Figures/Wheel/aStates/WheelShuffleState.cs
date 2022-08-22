using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;

using Orazum.Collections;
using static Orazum.Math.MathUtilities;

using rnd = Unity.Mathematics.Random;

public class WheelShuffleState : FigureState
{ 
    private const int FastSteps = 0;
    private const float FastSpeed = 10;
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

    private Wheel _wheel;

    public WheelShuffleState(FigureParamsSO figureParams, Wheel wheelArg)
    {
        _wheel = wheelArg;

        _shuffleLerpSpeed = figureParams.ShuffleLerpSpeed;
        _shufflePauseTime = figureParams.ShufflePauseTime;

        _shuffleStepsAmount = figureParams.ShuffleStepsAmount;
        _fastShuffleTime = 1 / FastSpeed + _shufflePauseTime / 10;
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

    public override FigureState HandleTransitions()
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
        if (_currentStep < FastSteps)
        {
            if (_currentShuffleTimer >= _fastShuffleTime)
            {
                Shuffle(FastSpeed);
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

        WheelRotationMove[] moves = new WheelRotationMove[_wheel.SideCount * _wheel.RingCount];
        int moveIndex = 0;
        for (int ring = 0; ring < _shuffleIndices.Length; ring++)
        {
            for (int side = 0; side < _shuffleIndices[ring].Length; side++)
            {
                int2 fromIndex = new int2(side, ring);
                int2 toIndex = _shuffleIndices[ring][side];
                WheelRotationMove rotationMove = new WheelRotationMove();
                rotationMove.AssignFromIndex(fromIndex);
                rotationMove.AssignToIndex(toIndex);
                rotationMove.AssignType(WheelRotationMove.TypeType.Clockwise);

                float rotationAngle = 0;
                int sideDeltaCW = Mathf.Abs(toIndex.x - fromIndex.x);
                int sideDeltaCCW = Mathf.Abs(toIndex.x + _wheel.SideCount - fromIndex.x);
                if (sideDeltaCW < sideDeltaCCW)
                { 
                    rotationAngle = sideDeltaCW * TAU / _wheel.SideCount * Mathf.Rad2Deg;
                    if (toIndex.x < fromIndex.x)
                    {
                        rotationAngle = -rotationAngle;
                        rotationMove.AssignType(WheelRotationMove.TypeType.CounterClockwise);
                    }
                }
                else
                { 
                    rotationAngle = sideDeltaCCW * TAU / _wheel.SideCount * Mathf.Rad2Deg;
                    if (toIndex.x + _wheel.SideCount < fromIndex.x)
                    {
                        rotationAngle = -rotationAngle;
                        rotationMove.AssignType(WheelRotationMove.TypeType.CounterClockwise);
                    }
                }
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

    private WheelShuffleState GetThisState()
    {
        return this;
    }
}