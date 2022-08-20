using Unity.Collections;

using Orazum.Meshing;

public class ValknutSegment : FigureSegment
{
    protected override void InitializeMover()
    {
        _mover = gameObject.AddComponent<ValknutSegmentMover>();
    }

    /// <summary>
    /// Odd puzzleIndex means that it is twoAngleSegment which has 3 stripSegments out of max 4,
    /// because valknutSegmentMover should have the minimum largest needed NativeArray,
    /// we extend its size to include additional 
    /// </summary>
    public override void Initialize(NativeArray<VertexData> verticesArg, int puzzleIndexArg)
    {
        base.Initialize(verticesArg, puzzleIndexArg);
        _puzzleIndex = puzzleIndexArg;
        if (_puzzleIndex % 2 == 0)
        { 

        }
    }
}