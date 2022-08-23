using System;

public static class FigureDelegatesContainer
{
    public static Action<Figure> ActionCheckFigureCompletion;
    public static Action<FigureSegment[]> EventSegmentsWereEmptied;
    public static Action FigureCompleted;
}