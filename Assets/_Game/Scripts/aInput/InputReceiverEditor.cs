using UnityEngine;

public class InputReceiverEditor : MonoBehaviour
{
#if UNITY_EDITOR// && !UNITY_ANDROID && !UNITY_IOS
    [SerializeField]
    [Range(0, 0.5f)]
    private float _swipeThreshold;

    private Camera _renderingCamera;

    private bool _isSwiping;
    private Vector2 _pressPos;
    private Vector2 _lastPos;

    private SwipeCommand _swipeCommand;

    private void Awake()
    {
        _swipeCommand = new SwipeCommand();
    }

    private void Start()
    {
        _renderingCamera = InputDelegatesContainer.GetRenderingCamera();
    }

    private SwipeCommand GetCurrentSwipeCommand()
    {
        return _swipeCommand;
    }

    private void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            if (_isSwiping)
            {
                Vector2 viewStartPos = _renderingCamera.ScreenToViewportPoint(_pressPos);
                Vector2 viewEndPos = _renderingCamera.ScreenToViewportPoint(_lastPos);
                 
                Vector2 delta = viewEndPos - viewStartPos;
                if (delta.sqrMagnitude >= _swipeThreshold * _swipeThreshold)
                {
                    _swipeCommand.SetViewStartPos(viewStartPos);
                    _swipeCommand.SetViewEndPos(viewEndPos);
                    InputDelegatesContainer.SwipeCommand?.Invoke(_swipeCommand);
                }
                
                _isSwiping = false;
            }
            return;
        }

        if (_isSwiping)
        {
            _lastPos = Input.mousePosition;
        }
        else
        {
            _isSwiping = true;
            _pressPos = Input.mousePosition;
        }
    }
#endif
}