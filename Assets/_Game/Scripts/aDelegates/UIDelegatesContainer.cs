using System;
using UnityEngine;
using Orazum.UI;

public static class UIDelegatesContainer
{
    // UIEventsUpdater
    public static Func<UIPointerEventsUpdater> GetEventsUpdater;

    public static Action ShowEndLevelCanvas;
}