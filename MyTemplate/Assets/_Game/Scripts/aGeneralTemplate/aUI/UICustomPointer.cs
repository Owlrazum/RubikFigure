using UnityEngine;
using UnityEngine.EventSystems;

public class UICustomPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private IPointerCallback callback;
    public void AssignPointerCallback(IPointerCallback callbalkArg)
    {
        callback = callbalkArg;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        callback.OnPointerDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        callback.OnPointerUp();
    }
}
