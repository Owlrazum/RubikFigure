using System;

public static class FigureDelegatesContainer
{
    public static Action FinishMeshGeneration;
    public static Action StartShuffleTransitionsGeneration;
    public static Action FinishShuffleTransitionsGeneration;

    public static Func<Figure> GetFigure;

    public static Action<Figure> ActionCheckCompletion;
    public static Action<FigureSegment[]> EventSegmentsWereEmptied;
    public static Action Completed;
}