using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class FigureStatesController : MonoBehaviour
{ 
    protected FigureState _currentState;

    public abstract void Initialize(Figure figureArg, FigureParamsSO figureParams, int2[] emptyIndices);

    protected IEnumerator StateSwitchSequence()
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