using System;
using UnityEngine;

public static class GameDelegatesContainer
{
    public static Func<GameStateType> GetGameState;

    public static Action StartCameraTransitionTo;

    public static Action<LevelDescriptionSO> StartLevel;
    public static Action CompleteLevel;
    public static Action FailLevel;

    public static Action<LevelDescriptionSO> EventLevelStarted;
    public static Action EventLevelCompleted;
    public static Action EventLevelFailed;

    public static Func<Camera> GetRenderingCamera;

    public static Func<bool> GetShouldShowTutorial;
}