using UnityEngine.Assertions;

public class WheelSegment : FigureSegment
{ 
    protected override void InitializeMover()
    {
        _mover = gameObject.AddComponent<WheelSegmentMover>();
    }
}