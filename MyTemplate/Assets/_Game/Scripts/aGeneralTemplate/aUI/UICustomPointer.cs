using System;

using UnityEngine;
using UnityEngine.EventSystems;

public class UICustomPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    protected Action callback;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        callback?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        callback?.Invoke();
    }
}
