using Unity.Mathematics;

using UnityEngine;

public enum SelectMethodType
{ 
    Raycast
}
[CreateAssetMenu(fileName = "FigureParams", menuName = "Figure/FigureParams", order = 1)]
public class FigureParamsSO : ScriptableObject
{
    public FigureGenParamsSO GenParams;
    public SelectMethodType SelectMethod;

    [Header("Emptying")]
    public float EmptyLerpSpeed = 1;
    public float BeforeEmptyTime = 1;

    public bool ShouldUsePredefinedEmptyPlaces = false;
    [SerializeField]
    private int _emptyPlacesCount = 2;
    public int EmptyPlacesCount
    {
        get { return ShouldUsePredefinedEmptyPlaces ? PredefinedEmptyPlaces.Length : _emptyPlacesCount; }
    }
    public int2[] PredefinedEmptyPlaces;

    [Header("Shuffling")]
    public int ShuffleStepsAmount = 1;
    public float ShufflePauseTime = 0.5f;
    public float ShuffleLerpSpeed = 1;
    
    [Header("Moving")]
    public float MoveLerpSpeed = 2;
    public float SelectionScaling = 1.2f;

    [Header("Completion")]
    public float CompleteLerpSpeed;
    public float RotationAmplitude;
}