using UnityEngine;
using Orazum.Utilities.ConstContainers;

public class InputReceiverEditor : MonoBehaviour
{
    [SerializeField]
    [Range(0, 0.5f)]
    private float _swipeThreshold;
#if UNITY_EDITOR// && !UNITY_ANDROID && !UNITY_IOS

    private Camera _renderingCamera;

    private Vector2 _pressPos;
    private Vector2 _lastPos;

    private SwipeCommand _swipeCommand;
    private bool _isSegmentSelected;

    private bool _shouldRespond;

    private void Awake()
    {
        _swipeCommand = new SwipeCommand();

        _shouldRespond = true;
        InputDelegatesContainer.SetShouldRespond += SetShouldRespond;
    }

    private void OnDestroy()
    { 
        InputDelegatesContainer.SetShouldRespond -= SetShouldRespond;
    }

    private void SetShouldRespond(bool value)
    {
        _shouldRespond = value;
    }

    private void Start()
    {
        _renderingCamera = InputDelegatesContainer.GetRenderingCamera();
    }

    private void Update()
    {
        if (!_shouldRespond)
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        { 
            _pressPos = Input.mousePosition;
            Ray ray = _renderingCamera.ScreenPointToRay(_pressPos);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000,
               LayerUtilities.SEGMENT_POINTS_LAYER_MASK, QueryTriggerInteraction.Collide))
            {
                _isSegmentSelected = true;
                InputDelegatesContainer.SelectSegmentCommand?.Invoke(hitInfo.collider);
            }
        }

        if (Input.GetMouseButton(0))
        {
            _lastPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_isSegmentSelected)
            {
                _isSegmentSelected = false;

                Vector2 viewStartPos = _renderingCamera.ScreenToViewportPoint(_pressPos);
                Vector2 viewEndPos = _renderingCamera.ScreenToViewportPoint(_lastPos);

                Vector2 delta = viewEndPos - viewStartPos;
                if (delta.sqrMagnitude >= _swipeThreshold * _swipeThreshold)
                {
                    _swipeCommand.SetViewStartPos(viewStartPos);
                    _swipeCommand.SetViewEndPos(viewEndPos);
                    InputDelegatesContainer.SwipeCommand?.Invoke(_swipeCommand);
                    return;
                }
            }
            InputDelegatesContainer.DeselectSegmentCommand?.Invoke();
        }
    }
#endif
}