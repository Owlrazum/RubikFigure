using UnityEngine;

[CreateAssetMenu(fileName = "LevelDescription", menuName = "Game/LevelDescription", order = 1)]
public class LevelDescriptionSO : ScriptableObject
{
    public int sceneIndex;
    public FigureParamsSO FigureParams;
    public FigureGenParamsSO GenParams;
}