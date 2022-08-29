using Unity.Mathematics;

using UnityEngine;

public class RaycastSelectable : Selectable
{
    private int _layerMask;
    private float _maxDistance;
    private QueryTriggerInteraction _triggerInteraction;

    public RaycastSelectable(int layerMask)
    { 
        _layerMask = layerMask;
        _maxDistance = 1000;
        _triggerInteraction = QueryTriggerInteraction.Collide;
    }

    public void SetParams(float maxDistance, QueryTriggerInteraction triggerInteraction)
    {
        _maxDistance = maxDistance;
        _triggerInteraction = triggerInteraction;
    }

    public override void CheckSelectionOnPointerDown(float3x2 cameraRay)
    {
        Debug.DrawRay(cameraRay[0], cameraRay[1] * 100, Color.red, 2);
        if (Physics.Raycast(cameraRay[0], cameraRay[1], out RaycastHit hitInfo, 1000,
               _layerMask, _triggerInteraction))
        {
            _selectAction?.Invoke(hitInfo.collider);
        }
    }

    public override void CheckDeselectionOnPointerUp(float3x2 cameraRay)
    {
        if (_deselectAction != null)
        {
            if (Physics.Raycast(cameraRay[0], cameraRay[1], out RaycastHit hitInfo, 1000,
                _layerMask, _triggerInteraction))
            {
                if (_deselectCheck.Invoke(hitInfo.collider))
                {
                    _deselectAction.Invoke();
                }
            }
            else
            {
                if (_deselectCheck.Invoke(null))
                {
                    _deselectAction.Invoke();
                }
            }
        }
    }
}
