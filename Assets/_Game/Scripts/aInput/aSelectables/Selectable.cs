using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class Selectable : UnityEngine.Object
{
    public abstract void CheckSelectionOnPointerDown(float3x2 cameraRay);
    public abstract void CheckDeselectionOnPointerUp(float3x2 cameraRay);
    public void CheckSelectionOnPointerDown(Ray ray)
    {
        CheckSelectionOnPointerDown(new float3x2(ray.origin, ray.direction));
    }
    public void CheckDeselectionOnPointerUp(Ray ray)
    {
        CheckDeselectionOnPointerUp(new float3x2(ray.origin, ray.direction));
    }

    protected Action<Collider> _selectAction;
    protected Action _deselectAction;
    protected Func<Collider, bool> _deselectCheck;
    public void SetSelectionActions(
        Action<Collider> selectAction,
        Func<Collider, bool> deselectCheck,
        Action deselectAction)
    {
        Assert.IsFalse(deselectAction != null && deselectCheck == null, "Deselect action requires deselect check");
        _selectAction = selectAction;
        _deselectCheck = deselectCheck;
        _deselectAction = deselectAction;
    }
}