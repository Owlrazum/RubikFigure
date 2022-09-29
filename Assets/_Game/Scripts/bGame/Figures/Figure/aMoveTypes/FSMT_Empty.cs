using Unity.Mathematics;

public class FSMT_Empty : FSM_Transition
{
    public int2 Index { get; private set; }
    public void AssignIndex(int2 index)
    {
        Index = index;
    }
}
