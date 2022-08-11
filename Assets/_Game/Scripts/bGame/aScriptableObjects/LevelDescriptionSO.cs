using UnityEngine;

[CreateAssetMenu(fileName = "LevelDescription", menuName = "Game/LevelDescription", order = 1)]
public class LevelDescriptionSO : ScriptableObject
{
    public WheelGenParamsSO GenerationParams;
    public GameObject SegmentPrefab;
    public GameObject SegmentPointPrefab;

    public int ShuffleStepsAmount;
    public float ShufflePauseTime;
    public float ShuffleLerpSpeed;
    public float MoveLerpSpeed;

    public bool ShouldUsePredefinedEmptyPlaces;
    public Vector2Int[] PredefinedEmptyPlaces;
    public int EmptyPlacesCount;
}