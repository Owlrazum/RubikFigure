using System;

public static class ApplicationDelegatesContainer
{ 
    public static Action<int> StartLoadingScene;
    public static Action EventStartedLoadingScene;
    public static Action<Action> FinishLoadingScene;
    
    public static Action<Action> LoadMainMenu;

    public static Func<float> GetSceneLoadingProgress;

    // public static Action EventBeforeFinishingLoadingScene;
}