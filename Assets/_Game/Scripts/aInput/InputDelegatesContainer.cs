using System;
using UnityEngine;

public static class InputDelegatesContainer
{
    public static Action<Selectable> RegisterSelectable;
    public static Action<Selectable> UnregisterSelectable;
    public static Action<bool> SetShouldRespond;
    
    public static Action<Camera> SetInputCamera;
    public static Func<Camera> GetInputCamera;

    public static Action<SwipeCommand> SwipeCommand;
    public static Action<TouchDownCommand> TouchDownCommand;
    public static Action<TouchUpCommand> TouchUpCommand;
    public static Action<JoystickCommand> JoystickCommand;
}