using System.Collections;
using UnityEngine;

public abstract class FigureStatesController : MonoBehaviour
{ 
    protected FigureState _currentState;

    public FigureIdleState IdleState { get; protected set; }
    public FigureMoveState MoveState { get; protected set; }
    public FigureShuffleState ShuffleState { get; protected set; }

    public abstract void Initialize(Figure figure, FigureParamsSO figureParams);
    public void StartUpdating()
    { 
        _currentState = ShuffleState;
        _currentState.OnEnter();

        StartCoroutine(StateSwitchSequence());
    }

    private IEnumerator StateSwitchSequence()
    {
        while (true)
        {
            FigureState newState = _currentState.HandleTransitions();
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