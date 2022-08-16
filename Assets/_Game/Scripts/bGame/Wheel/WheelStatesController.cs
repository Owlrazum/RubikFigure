using System.Collections;

using UnityEngine;

public class WheelStatesController : MonoBehaviour
{
    private WheelState _currentState;
    private Wheel _wheel;

    public void GenerationInitialization(Wheel wheelArg, WheelGenerationData generationData)
    {
        _wheel = wheelArg;

        IdleState idleState = new IdleState(generationData.LevelDescription, _wheel);
        MoveState moveState = new MoveState(generationData.LevelDescription, _wheel);
        ShuffleState shuffleState = new ShuffleState(generationData.LevelDescription, _wheel);
        shuffleState.PrepareForShuffle(generationData.EmtpySegmentPointIndicesForShuffle);

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