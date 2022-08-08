using System.Collections;

using UnityEngine;

public class WheelController : MonoBehaviour
{
    private WheelState _currentState;
    private Wheel _wheel;

    public void GenerationInitialization(Wheel wheelArg, WheelGenerationData generationData)
    {
        _wheel = wheelArg;

        IdleState idleState = new IdleState(generationData);
        MoveState moveState = new MoveState(generationData);
        ShuffleState shuffleState = new ShuffleState(generationData);

        _currentState = shuffleState;

        StartCoroutine(StateSwitchSequence());
    }

    private IEnumerator StateSwitchSequence()
    {
        while (true)
        {
            WheelState newState = _currentState.HandleTransitions();
            if (newState != null)
            {
                newState.OnEnter();
                _currentState = newState;
            }
            
            _currentState.StartProcessingState(_wheel);
            yield return null;
        }
    }
}