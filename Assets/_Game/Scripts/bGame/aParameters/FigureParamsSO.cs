using Unity.Mathematics;

using UnityEngine;

public enum SelectMethodType
{ 
    Raycast
}
[CreateAssetMenu(fileName = "FigureParams", menuName = "Figure/FigureParams", order = 1)]
public class FigureParamsSO : ScriptableObject
{
    public FigureGenParamsSO FigureGenParamsSO;
    public SelectMethodType SelectMethod;

    public int ShuffleStepsAmount = 1;
    public float ShufflePauseTime = 0.5f;
    public float ShuffleLerpSpeed = 1;
    public float MoveLerpSpeed = 2;

    public Vector3 StartPositionForSegmentsInCompletionPhase = new Vector3(0, -10, 0);

    public bool ShouldUsePredefinedEmptyPlaces = false;
    public int2[] PredefinedEmptyPlaces;
    public int EmptyPlacesCount = 2;
}