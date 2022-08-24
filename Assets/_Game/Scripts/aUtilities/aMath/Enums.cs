namespace Orazum.Math
{ 
    public enum ClockOrderType
    { 
        CW, // Clockwise
        CCW // CounterClockwise
    }

    public static class ClockOrderConversions
    { 
        public static ClockOrderType IntToClockOrder(int i)
        {
            if (i >= 0)
            {
                return ClockOrderType.CW;
            }

            return ClockOrderType.CCW;
        }

        public static int ClockOrderToInt(ClockOrderType clockOrder)
        {
            switch (clockOrder)
            { 
                case ClockOrderType.CW:
                    return 10;
                case ClockOrderType.CCW:
                    return -10;
            }

            throw new System.ArgumentOutOfRangeException("Unknown type of clockOrder");
        }
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

    public enum DirectionOrderType
    { 
        Start,
        End
    }
}