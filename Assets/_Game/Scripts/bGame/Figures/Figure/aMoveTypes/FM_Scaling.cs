public class FM_Scaling : FMS_Transition
{
    public FS_Scaler Scaler { get; protected set; }
    public void AssignScaler(FS_Scaler scaler)
    {
        Scaler = scaler;
    }
}
