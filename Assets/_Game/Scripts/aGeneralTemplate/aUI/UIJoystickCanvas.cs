public class UIJoysticCanvas : UIBaseFadingCanvas
{
    protected override void Awake()
    {
        base.Awake();

        //EventsContainer.PlayerCameraShotActive += ShowItself;
    }

    private void OnDestroy()
    { 
        //EventsContainer.PlayerCameraShotActive -= ShowItself;
    }

}
