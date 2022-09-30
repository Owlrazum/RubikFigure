public class FMS_Scaling : FigureMoveOnSegment
{
    public FS_Scaler Scaler { get; protected set; }
    public void AssignScaler(FS_Scaler scaler)
    {
        Scaler = scaler;
    }
}
