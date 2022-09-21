using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class InputReceiver : MonoBehaviour
{
    [SerializeField]
    [Range(0, 0.5f)]
    protected float _swipeThreshold;

    protected HashSet<Selectable> _registeredSelectables;
    
    protected Camera _inputCamera;

    protected float3 _pressPos;
    protected float3 _lastPos;

    protected abstract void CheckPointerDown();
    protected abstract void CheckPointer();
    protected abstract void CheckPointerUp();
    protected void CheckSwipeCommand()
    {
        float3 viewStartPos = _inputCamera.ScreenToViewportPoint(_pressPos);
        float3 viewEndPos = _inputCamera.ScreenToViewportPoint(_lastPos);

        float3 delta = viewEndPos - viewStartPos;
        if (math.lengthsq(delta) >= _swipeThreshold * _swipeThreshold)
        {
            InputDelegatesContainer.SwipeCommand?.Invoke(new SwipeCommand(viewStartPos.xy, viewEndPos.xy));
        }
    }

    private bool _shouldUpdate;

    private void Awake()
    {
        _shouldUpdate = true;
        _registeredSelectables = new HashSet<Selectable>();
        InputDelegatesContainer.SetShouldRespond += SetShouldRespond;
        InputDelegatesContainer.RegisterSelectable += RegisterSelectable;
        InputDelegatesContainer.UnregisterSelectable += UnregisterSelectable;
    }

    private void OnDestroy()
    {
        InputDelegatesContainer.SetShouldRespond -= SetShouldRespond;
        InputDelegatesContainer.RegisterSelectable -= RegisterSelectable;
        InputDelegatesContainer.UnregisterSelectable -= UnregisterSelectable;
    }

    private void Start()
    {
        _inputCamera = InputDelegatesContainer.GetInputCamera();
    }

    private void SetShouldRespond(bool shouldRespond)
    {
        _shouldUpdate = shouldRespond;
    }

    private void RegisterSelectable(Selectable selectable)
    {
        Assert.IsTrue(!_registeredSelectables.Contains(selectable));
        _registeredSelectables.Add(selectable);
    }

    private void UnregisterSelectable(Selectable selectable)
    { 
        Assert.IsTrue(_registeredSelectables.Contains(selectable));
        _registeredSelectables.Remove(selectable);
    }

    private void Update()
    {
        if (!_shouldUpdate)
        {
            return;
        }

        CheckPointerDown();
        CheckPointer();
        CheckPointerUp();
    }
}