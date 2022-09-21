using Unity.Mathematics;
using UnityEngine;

public class InputReceiverMobile : InputReceiver
{
    private bool _isTouching;
    protected override void CheckPointerDown()
    {
        if (Input.touchCount == 1 && !_isTouching)
        {
            _isTouching = true;
            _pressPos = new float3(Input.GetTouch(0).position, 0);

            Ray ray = _inputCamera.ScreenPointToRay(_pressPos);
            foreach (Selectable s in _registeredSelectables)
            {
                s.CheckSelectionOnPointerDown(ray);
            }
        }
    }
    protected override void CheckPointer()
    {
        if (Input.touchCount == 1 && _isTouching)
        {
            _lastPos = new float3(Input.GetTouch(0).position, 0);
        }
    }
    protected override void CheckPointerUp()
    {
        if (_isTouching && Input.touchCount == 0)
        { 
            _isTouching = false;
            CheckSwipeCommand();

            Ray ray = _inputCamera.ScreenPointToRay(_lastPos);
            foreach (Selectable s in _registeredSelectables)
            {
                s.CheckDeselectionOnPointerUp(ray);
            }
        }
    }
}