using System;
using Unity.Mathematics;

public static class FigureDelegatesContainer
{
    public static Action<LevelDescriptionSO> ActionGrabParameters;
    
    public static Action ActionStartFigureGeneration;
    public static Action<Figure> EventFigureGenerationCompleted;

    public static Action<Figure> ActionStartPuzzle;
    
    public static Action ActionCheckCompletion;
    public static Action Completed;
}