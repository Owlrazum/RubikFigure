using Unity.Mathematics;
using UnityEngine;

public class InputReceiverEditor : InputReceiver
{
    protected override void CheckPointerDown()
    {
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
        if (Input.GetMouseButton(0))
        {
            _lastPos = Input.mousePosition;
        }
    }
    protected override void CheckPointerUp()
    {
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

    private void CheckSwipeCommand()
    {
        float3 viewStartPos = _inputCamera.ScreenToViewportPoint(_pressPos);
        float3 viewEndPos = _inputCamera.ScreenToViewportPoint(_lastPos);

        float3 delta = viewEndPos - viewStartPos;
        if (math.lengthsq(delta) >= _swipeThreshold * _swipeThreshold)
        {
            InputDelegatesContainer.SwipeCommand?.Invoke(new SwipeCommand(viewStartPos.xy, viewEndPos.xy));
        }
    }
}