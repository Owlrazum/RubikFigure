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

    public static Action<TouchCommand> TouchCommanded;
    public static void OnTouchCommanded(TouchCommand touchCommand)
    {
        TouchCommanded?.Invoke(touchCommand);
    }

    public static Action ProgressWasMade;
    public static void OnProgressWasMade()
    {
        ProgressWasMade?.Invoke();
    }

    public static Action GameEnd;
    public static void OnGameEnd()
    {
        GameEnd?.Invoke();
    }
}