using System;
using Unity.Mathematics;

public static class FigureDelegatesContainer
{
    public static Action<Figure> EventFigureGenerationCompleted;
    public static Action ActionCheckCompletion;
    public static Action Completed;
}