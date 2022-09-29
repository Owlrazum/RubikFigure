using UnityEngine;

public class InputReceiverEditor : InputReceiver
{
    protected override void CheckPointerDown()
    {
        if (_inputCamera == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _pressPos = Input.mousePosition;
            
            Ray ray = _inputCamera.ScreenPointToRay(_pressPos);
            foreach (Selectable s in _registeredSelectables)
            {
                s.CheckSelectionOnPointerDown(ray);
            }
        }
    }
    protected override void CheckPointer()
    {
        if (_inputCamera == null)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            _lastPos = Input.mousePosition;
        }
    }
    protected override void CheckPointerUp()
    {
        if (_inputCamera == null)
        {
            return;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            CheckSwipeCommand();

            Ray ray = _inputCamera.ScreenPointToRay(_pressPos);
            foreach (Selectable s in _registeredSelectables)
            {
                s.CheckDeselectionOnPointerUp(ray);
            }
        }
    }
}