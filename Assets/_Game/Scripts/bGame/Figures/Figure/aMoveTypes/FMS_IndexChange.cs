using Unity.Mathematics;

public class FMS_IndexChange : FM_Segment
{
    public int2 From
    {
        get { return Index; }
        protected set { Index = value; }
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

    public FMS_IndexChange()
    {
        From = -1;
        To = -1;
    }

    public override string ToString()
    {
        return From + " " + To;
    }
}