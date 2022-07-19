using UnityEngine;

public enum CubeTurnType
{ 
    Forward,
    Left,
    Right,
    Backward
}
[CreateAssetMenu(fileName = "LevelDescription", menuName = "Game/LevelDescription", order = 1)]
public class LevelDescriptionSO : ScriptableObject
{
    public CubeTurnType[] CubeTurnSequence;
}