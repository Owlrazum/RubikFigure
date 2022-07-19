using System;
using UnityEngine;
using Orazum.UI;

public static class UIDelegatesContainer
{
    // UIEventsUpdater
    public static Func<UIEventsUpdater> GetEventsUpdater;

    public static Action ShowEndLevelCanvas;
}