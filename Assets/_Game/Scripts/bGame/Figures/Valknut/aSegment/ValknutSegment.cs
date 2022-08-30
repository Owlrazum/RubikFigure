public class ValknutSegment : FigureSegment
{
    protected override void InitializeMover()
    {
        _mover = gameObject.AddComponent<ValknutSegmentMover>();
    }
}