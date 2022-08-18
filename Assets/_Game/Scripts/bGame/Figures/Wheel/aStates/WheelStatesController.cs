using System.Collections;

using Unity.Mathematics;

using UnityEngine;

public class WheelStatesController : MonoBehaviour
{
    private WheelState _currentState;
    private Wheel _wheel;

    public void Initialize(Wheel wheelArg, FigureParamsSO figureParams, int2[] emptyIndices)
    {
        _wheel = wheelArg;

        IdleState idleState = new IdleState(figureParams, _wheel);
        MoveState moveState = new MoveState(figureParams, _wheel);
        ShuffleState shuffleState = new ShuffleState(figureParams, _wheel);
        shuffleState.PrepareForShuffle(emptyIndices);

        _currentState = shuffleState;
        _currentState.OnEnter();

        StartCoroutine(StateSwitchSequence());
    }

    private IEnumerator StateSwitchSequence()
    {
        while (true)
        {
            WheelState newState = _currentState.HandleTransitions();
            if (newState != null)
            {
                do
                {
                    _currentState.OnExit(); 
                    _currentState = newState;
                    _currentState.OnEnter();
                    newState = _currentState.HandleTransitions();
                } while (newState != null);
                Debug.Log(_currentState);
            }
            
            _currentState.ProcessState();
            yield return null;
        }
    }
}