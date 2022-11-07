using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class FigureStatesController : MonoBehaviour
{ 
    protected FigureState _currentState;

    public FigureIdleState IdleState { get; protected set; }
    public FigureMoveState MoveState { get; protected set; }
    public FigureShuffleState ShuffleState { get; protected set; }

    private IEnumerator _stateSwitchSequence;
    public abstract void Initialize(Figure figure, FigureParamsSO figureParams, FigureGenParamsSO genParams);
    public void StartUpdating()
    {
        _currentState = ShuffleState;
        _currentState.OnEnter();

        Assert.IsTrue(_stateSwitchSequence == null);
        _stateSwitchSequence = StateSwitchSequence();
        StartCoroutine(_stateSwitchSequence);
    }

    public void StopUpdating()
    {
        Assert.IsTrue(_stateSwitchSequence != null);
        StopCoroutine(_stateSwitchSequence);
        _stateSwitchSequence = null;
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