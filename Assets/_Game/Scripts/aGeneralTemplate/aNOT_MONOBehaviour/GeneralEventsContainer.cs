using System;
using GeneralTemplate;

public static class GeneralEventsContainer
{
    public static Action Initialization;
    public static Action GameStart;
    public static Action GameEnd;

    public static Action<LevelData> LevelLoaded;

    #region InputCommands
    public static Action<InputCommand> InputCommanded;
    public static Action<TouchCommand> TouchCommanded;
    public static Action<JoystickCommand> JoystickCommanded;
    public static Action<DragCommand> DragCommanded;
    #endregion
    
    public static Action ProgressWasMade;
}