namespace Orazum.Math
{ 
    /// The code assumes that there are only two types of each of the following enums, be careful with adding a new one. 
    /// Example, instead of checking if it equals to the second type, there are else keywords instead of else if (equal to second type)
    public enum ClockOrderType
    { 
        CW, // Clockwise
        AntiCW // CounterClockwise
    }

    public enum VertOrderType
    { 
        Up,
        Down
    }

    public enum HorizOrderType
    { 
        Right,
        Left
    }

    public enum LineEndType
    { 
        Start,
        End
    }

    public enum LineEndDirectionType
    { 
        StartToEnd,
        EndToStart
    }
}