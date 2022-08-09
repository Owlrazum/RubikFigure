using UnityEngine;

public class InputReceiverMobile : MonoBehaviour
{
#if !UNITY_EDITOR && UNITY_ANDROID && UNITY_IOS
    [SerializeField]
    [Range(0, 0.5f)]
    private float _swipeThreshold;

    [SerializeField]
    private Camera _renderingCamera;

    private bool _isSwiping;
    private Vector2 _pressPos;
    private Vector2 _lastPos;

    private void Update()
    {
        if (Input.touchCount != 1)
        {
            if (_isSwiping)
            {
                Vector2 viewStartPos = _renderingCamera.ScreenToViewportPoint(_pressPos);
                Vector2 viewEndPos = _renderingCamera.ScreenToViewportPoint(_lastPos);
                 
                Vector2 delta = viewEndPos - viewStartPos;
                if (delta.sqrMagnitude >= _swipeThreshold * _swipeThreshold)
                {
                    SwipeCommand swipeCommand = new SwipeCommand(delta.normalized, _pressPos);
                    InputDelegatesContainer.SwipeCommand?.Invoke(swipeCommand);
                }
                
                _isSwiping = false;
            }
            return;
        }

        Touch touch = Input.GetTouch(0);
        if (_isSwiping)
        {
            _lastPos = touch.position;
        }
        else
        {
            _isSwiping = true;
            _pressPos = touch.position;
        }
    }
#endif
}