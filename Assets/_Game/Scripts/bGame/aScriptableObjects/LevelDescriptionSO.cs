using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "LevelDescription", menuName = "Game/LevelDescription", order = 1)]
public class LevelDescriptionSO : ScriptableObject
{
    public bool ShouldUsePredefinedEmptyPlaces;
    public int2[] PredefinedEmptyPlaces;
    public int EmptyPlacesCount;

    public int ShuffleStepsAmount;
    public float ShufflePauseTime;
    public float ShuffleLerpSpeed;
    public float MoveLerpSpeed;
}