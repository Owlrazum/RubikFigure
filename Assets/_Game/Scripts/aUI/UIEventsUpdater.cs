using System.Collections.Generic;
using UnityEngine;

namespace Orazum.UI 
{
    public class UIEventsUpdater : MonoBehaviour
    {
        private const float SQR_MAGNITUDE_POINTER_TOUCH_TOLERANCE = 10;

        [SerializeField]
        private float _offset = 1;

        private Dictionary<int, IPointerDownUpHandler> _downUpHandlers;
        private Dictionary<int, IPointerEnterExitHandler> _enterExitHandlers;
        
        private Dictionary<int, IPointerTouchHandler> _touchHandlers;
        private Dictionary<int, IPointerLocalPointHandler> _localPointHandlers;

        private int _movingUICount = 0;
        private int _finishedMovingUICount = 0;
        private bool _isBeganTouchValid;

#if UNITY_STANDALONE
        private Vector2 _pressMousePos;
        private Vector2 _currentMousePos;
#elif UNITY_ANDROID || UNITY_IOS
        private Touch _currentTouch;
#endif

        private void Awake()
        {
            _downUpHandlers     = new Dictionary<int, IPointerDownUpHandler>();
            _enterExitHandlers  = new Dictionary<int, IPointerEnterExitHandler>();

            _touchHandlers      = new Dictionary<int, IPointerTouchHandler>();
            _localPointHandlers = new Dictionary<int, IPointerLocalPointHandler>();

            UIDelegatesContainer.GetEventsUpdater += GetUpdater;
        }

        private void OnDestroy()
        {
            _downUpHandlers.Clear();
            _enterExitHandlers.Clear();

            _touchHandlers.Clear();
            _localPointHandlers.Clear();
            
            UIDelegatesContainer.GetEventsUpdater -= GetUpdater;
        }

        private UIEventsUpdater GetUpdater()
        {
            return this;
        }


        public void AddPointerDownUpHandler(IPointerDownUpHandler handler)
        {
            handler.DownUpState = false;
            _downUpHandlers.Add(handler.InstanceID, handler);
        }
        public void AddPointerEnterExitHandler(IPointerEnterExitHandler handler)
        {
            handler.EnterState = false;
            _enterExitHandlers.Add(handler.InstanceID, handler);
        }
        public void AddPointerTouchHandler(IPointerTouchHandler handler)
        {
            _touchHandlers.Add(handler.InstanceID, handler);
        }
        public void AddPointerLocalPointHandler(IPointerLocalPointHandler handler)
        {
            _localPointHandlers.Add(handler.InstanceID, handler);
        }


        public void RemovePointerDownUpHandler(IPointerDownUpHandler handler)
        {
            _downUpHandlers.Remove(handler.InstanceID);
        }
        public void RemovePointerEnterExitHandler(IPointerEnterExitHandler handler)
        {
            handler.EnterState = false;
            _enterExitHandlers.Remove(handler.InstanceID);
        }
        public void RemovePointerTouchHandler(IPointerTouchHandler handler)
        {
            _touchHandlers.Remove(handler.InstanceID);
        }
        public void RemovePointerLocalPointHandler(IPointerLocalPointHandler handler)
        {
            _localPointHandlers.Remove(handler.InstanceID);
        }


        private void RegisterMovingUI()
        {
            _movingUICount++;
        }
        private void UnregisterMovingUI()
        {
            _movingUICount--;
        }
        private void OnMovingUIFinishedMove()
        {
            _finishedMovingUICount++;
            if (_finishedMovingUICount == _movingUICount)
            {
#if UNITY_STANDALONE
                UpdatePointerHandlersStandalone();
#elif UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
                UpdatePointerHandlersMobile();
#endif
                _finishedMovingUICount = 0;
            }
        }


        private void Update()
        {
            if (_movingUICount == 0)
            {
#if UNITY_STANDALONE
                UpdatePointerHandlersStandalone();
#elif UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
                UpdatePointerHandlersMobile();
#endif
                _finishedMovingUICount = 0;
            }
        }

#if UNITY_STANDALONE
#region EditorOrStandalone
        private void UpdatePointerHandlersStandalone()
        {
            _currentMousePos = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                _isBeganTouchValid = true;
                _pressMousePos = Input.mousePosition;
                return;
            }
            if (_isBeganTouchValid)
            {
                if ((_currentMousePos - _pressMousePos).sqrMagnitude > SQR_MAGNITUDE_POINTER_TOUCH_TOLERANCE)
                {
                    _isBeganTouchValid = false;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_isBeganTouchValid)
                {
                    NotifyOnePointerTouchIfNeededStandalone();
                    _isBeganTouchValid = false;
                }
                else
                { 
                    NotifyManyPointerExitIfNeededStandalone();
                }
                return;
            }

            NotifyManyPointerExitIfNeededStandalone();
            NotifyManyPointerEnterIfNeededStandalone();
            NotifyLocalPointUpdateIfNeededStandalone();
        }

        private void NotifyOnePointerTouchIfNeededStandalone()
        { 
            foreach (var pair in _touchHandlers)
            {
                var handler = pair.Value;
                if (RectTransformUtility.RectangleContainsScreenPoint(handler.Rect, _currentMousePos))
                { 
                    handler.OnPointerTouch();
                    return;
                }
            }
        }

        private void NotifyManyPointerExitIfNeededStandalone()
        {
            foreach (var pair in _enterExitHandlers)
            {
                var handler = pair.Value;
                if (!RectTransformUtility.RectangleContainsScreenPoint(handler.InteractionRect, 
                    _currentMousePos, null, Vector4.one * _offset))
                {
                    if (handler.EnterState)
                    {
                        handler.EnterState = false;
                        handler.OnPointerExit();
                    }
                }
            }
        }

        private void NotyfyManyPointerExitWithNoTouchPosStandalone()
        { 
            foreach (var pair in _enterExitHandlers)
            {
                var handler = pair.Value;
                if (handler.EnterState)
                {
                    handler.EnterState = false;
                    handler.OnPointerExit();
                }
            }            
        }

        private void NotifyManyPointerEnterIfNeededStandalone()
        { 
            foreach (var pair in _enterExitHandlers)
            {
                var handler = pair.Value;
                if (RectTransformUtility.RectangleContainsScreenPoint(handler.InteractionRect, 
                    _currentMousePos, null, Vector4.one * _offset))
                {
                    if (!handler.EnterState)
                    {
                        handler.EnterState = true;
                        handler.OnPointerEnter();
                    }
                }
            }
        }

        private void NotifyLocalPointUpdateIfNeededStandalone()
        { 
            foreach (var pair in _localPointHandlers)
            {
                var handler = pair.Value;
                if (handler.ShouldUpdateLocalPoint)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(handler.Rect,
                        _currentMousePos, null, out Vector2 localPoint
                    );
                    handler.UpdateLocalPoint(new Vector2Int((int)localPoint.x, (int)localPoint.y));
                }
            }
        }
#endregion
#elif UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
#region Mobile
        private void UpdatePointerHandlersMobile()
        {
            _currentTouch = Input.GetTouch(0);
    
            if (_currentTouch.phase == TouchPhase.Began)
            {
                _isBeganTouchValid = true;
                return;
            }

            if (_isBeganTouchValid)
            {
                if (_currentTouch.deltaPosition.sqrMagnitude > SQR_MAGNITUDE_POINTER_TOUCH_TOLERANCE)
                {
                    _isBeganTouchValid = false;
                }
            }

            if (_currentTouch.phase == TouchPhase.Ended || _currentTouch.phase == TouchPhase.Canceled)
            {
                if (_isBeganTouchValid)
                {
                    NotifyOnePointerTouchIfNeededMobile();
                    _isBeganTouchValid = false;
                }
                else
                { 
                    NotifyManyPointerExitIfNeededMobile();
                }
                return;
            }

            NotifyManyPointerExitIfNeededMobile();
            NotifyManyPointerEnterIfNeededMobile();
            NotifyLocalPointUpdateIfNeededMobile();
        }

        private void NotifyOnePointerTouchIfNeededMobile()
        { 
            foreach (var pair in _touchHandlers)
            {
                var handler = pair.Value;
                if (RectTransformUtility.RectangleContainsScreenPoint(handler.Rect, _currentTouch.position))
                { 
                    handler.OnPointerTouch();
                    return;
                }
            }
        }

        private void NotifyManyPointerExitIfNeededMobile()
        {
            foreach (var pair in _enterExitHandlers)
            {
                var handler = pair.Value;
                if (!RectTransformUtility.RectangleContainsScreenPoint(handler.InteractionRect, 
                    _currentTouch.position, null, Vector4.one * _offset))
                {
                    if (handler.EnterState)
                    {
                        handler.EnterState = false;
                        handler.OnPointerExit();
                    }
                }
            }
        }

        private void NotyfyManyPointerExitWithNoTouchPosMobile()
        { 
            foreach (var pair in _enterExitHandlers)
            {
                var handler = pair.Value;
                if (handler.EnterState)
                {
                    handler.EnterState = false;
                    handler.OnPointerExit();
                }
            }            
        }

        private void NotifyManyPointerEnterIfNeededMobile()
        { 
            foreach (var pair in _enterExitHandlers)
            {
                var handler = pair.Value;
                if (RectTransformUtility.RectangleContainsScreenPoint(handler.InteractionRect,
                     _currentTouch.position, null, Vector4.one * _offset))
                {
                    if (!handler.EnterState)
                    {
                        handler.EnterState = true;
                        handler.OnPointerEnter();
                    }
                }
            }
        }

        private void NotifyLocalPointUpdateIfNeededMobile()
        { 
            foreach (var pair in _localPointHandlers)
            {
                var handler = pair.Value;
                if (handler.ShouldUpdateLocalPoint)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(handler.Rect,
                        _currentTouch.position, null, out Vector2 localPoint
                    );
                    handler.UpdateLocalPoint(new Vector2Int((int)localPoint.x, (int)localPoint.y));
                }
            }
        }
#endregion
#endif
    }
}