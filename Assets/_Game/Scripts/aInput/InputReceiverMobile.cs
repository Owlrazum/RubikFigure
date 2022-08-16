using UnityEngine;

using Orazum.Utilities.ConstContainers;

public class InputReceiverMobile : MonoBehaviour
{
    [SerializeField]
    [Range(0, 0.5f)]
    private float _swipeThreshold;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)

    private Camera _renderingCamera;

    private Vector2 _pressPos;
    private Vector2 _lastPos;

    private bool _isSwiping;
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

        if (Input.touchCount == 1)
        {
            Touch currentTouch = Input.GetTouch(0);
            if (!_isSwiping)
            {
                _isSwiping = true;
                _pressPos = currentTouch.position;
                Ray ray = _renderingCamera.ScreenPointToRay(_pressPos);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000,
                   LayerUtilities.SEGMENT_POINTS_LAYER_MASK, QueryTriggerInteraction.Collide))
                {
                    _isSegmentSelected = true;
                    InputDelegatesContainer.SelectSegmentCommand?.Invoke(hitInfo.collider);
                }
            }
            else
            { 
                _lastPos = currentTouch.position;
            }
        }
        else
        { 
            _isSwiping = false;
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