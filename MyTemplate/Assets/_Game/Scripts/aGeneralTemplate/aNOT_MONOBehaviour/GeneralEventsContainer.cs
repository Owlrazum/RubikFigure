using System;

public class GeneralEventsContainer
{
    public static Action Initialization;
    public static void OnInitialization()
    {
        Initialization?.Invoke();
    }

    public static Action GameStart;
    public static void OnGameStart()
    {
        GameStart?.Invoke();
    }

    

    

    public static Action GameEnd;
    public static void OnGameEnd()
    {
        GameEnd?.Invoke();
    }


    #region InputCommands
    public static Action<InputCommand> InputCommanded;
    public static void InvokeInputCommanded(InputCommand inputCommand)
    {
        InputCommanded?.Invoke(inputCommand);
    }

    public static Action<TouchCommand> TouchCommanded;
    public static void InvokeTouchCommanded(TouchCommand touchCommand)
    {
        TouchCommanded?.Invoke(touchCommand);
    }

    public static Action<JoystickCommand> JoystickCommanded;
    public static void InvokeInputCommanded(JoystickCommand joystickCommand)
    {
        InputCommanded?.Invoke(joystickCommand);
    }

    public static Action<DragCommand> DragCommanded;
    public static void InvokeInputCommanded(DragCommand dragCommand)
    {
        InputCommanded?.Invoke(dragCommand);
    }
    #endregion
    

    public static Action ProgressWasMade;
    public static void OnProgressWasMade()
    {
        ProgressWasMade?.Invoke();
    }
}