using System;
using UnityEngine;

public static class InputDelegatesContainer
{
    public static Action<bool> SetShouldRespond;
    
    public static Func<Camera> GetRenderingCamera;
    public static Func<Vector2> GetPointerPosition;

    public static Action StartGameCommand;
    public static Action ExitToMainMenuCommand;
    public static Action ExitGameCommand;

    public static Action ShuffleCommand;

    public static Action<Collider> SelectSegmentCommand;
    public static Action DeselectSegmentCommand;

    public static Action<SwipeCommand> SwipeCommand;
}