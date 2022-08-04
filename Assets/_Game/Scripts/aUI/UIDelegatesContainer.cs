using System;
using UnityEngine;
using Orazum.UI;

public static class UIDelegatesContainer
{
    public static Func<float> GetSceneLoadingProgress;

    // UIEventsUpdater
    public static Func<UIPointerEventsUpdater> GetEventsUpdater;

    public static Action ShowEndLevelCanvas;
}