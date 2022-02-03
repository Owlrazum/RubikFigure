using System;
using GeneralTemplate;

public static class GeneralEventsContainer
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

    public static Action<LevelData> LevelLoaded;
    public static void InvokeLevelLoaded(LevelData levelData)
    {
        LevelLoaded?.Invoke(levelData);
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
    public static void InvokeJoystickCommanded(JoystickCommand joystickCommand)
    {
        JoystickCommanded?.Invoke(joystickCommand);
    }

    public static Action<DragCommand> DragCommanded;
    public static void InvokeDragCommanded(DragCommand dragCommand)
    {
        DragCommanded?.Invoke(dragCommand);
    }
    #endregion
    

    public static Action ProgressWasMade;
    public static void OnProgressWasMade()
    {
        ProgressWasMade?.Invoke();
    }
}