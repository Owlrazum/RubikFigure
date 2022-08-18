using UnityEngine.Assertions;

public class WheelSegment : FigureSegment
{ 
    protected override void InitializeMover()
    {
        _mover = gameObject.AddComponent<WheelSegmentMover>();
    }

    public WheelSegmentMover GetSegmentMoverForTeleport()
    {
        var toReturn = _mover as WheelSegmentMover;
        Assert.IsNotNull(toReturn);
        return toReturn;
    }
}