using UnityEngine;

[CreateAssetMenu(fileName = "GameDesciption", menuName = "Game/GameDesciption", order = 1)]
public class GameDesciptionSO : ScriptableObject
{
    public LevelDescriptionSO[] Levels;
}