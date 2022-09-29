using Unity.Mathematics;

public class FSM_IndexChange : FS_Movement
{
    public int2 From
    {
        get { return SegmentIndex; }
        protected set { SegmentIndex = value; }
    }
    public void AssignFromIndex(int2 fromIndex)
    {
        From = fromIndex;
    }

    public int2 To { get; protected set; }
    public void AssignToIndex(int2 toIndex)
    {
        To = toIndex;
    }

    public FSM_IndexChange()
    {
        From = -1;
        To = -1;
    }

    public override string ToString()
    {
        return From + " " + To;
    }
}