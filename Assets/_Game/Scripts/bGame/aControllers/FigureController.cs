using UnityEngine;

public class FigureController : MonoBehaviour
{
    private void Awake()
    {
        GameDelegatesContainer.StartLevel += OnStartLevel;
        
        FigureDelegatesContainer.EventFigureGenerationCompleted += OnFigureGenerated;
    }

    private void OnDestroy()
    { 
        GameDelegatesContainer.StartLevel -= OnStartLevel;

        FigureDelegatesContainer.EventFigureGenerationCompleted -= OnFigureGenerated;
    }

    private void OnStartLevel(LevelDescriptionSO level)
    {
        FigureDelegatesContainer.ActionGrabParameters(level);
        FigureDelegatesContainer.ActionStartFigureGeneration();
    }

    private void OnFigureGenerated(Figure figure)
    {
        FigureDelegatesContainer.ActionStartPuzzle(figure);
    }
}