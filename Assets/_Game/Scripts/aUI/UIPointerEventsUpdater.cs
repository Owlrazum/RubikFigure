using System.Collections.Generic;
using UnityEngine;

namespace Orazum.UI 
{
    public class UIPointerEventsUpdater : MonoBehaviour
    {
        [SerializeField]
        private float _offset = 1;

        private Dictionary<int, IPointerTouchHandler> _touchHandlers;
        private Dictionary<int, IPointerDownUpHandler> _downUpHandlers;
        private Dictionary<int, IPointerEnterExitHandler> _enterExitHandlers;
        private Dictionary<int, IPointerLocalPointHandler> _localPointHandlers;

        private int _movingUICount = 0;
        private int _finishedMovingUICount = 0;

        private enum TouchStateType
        { 
            NoTouch,
            ChangedToNoTouch,
            ChangedToHasTouch,
            HasTouch
        }
        private TouchStateType _touchDownUpState;

        private Vector2 PointerPosition
        {
#if UNITY_EDITOR
            get { return Input.mousePosition; }
#elif UNITY_ANDROID
            get {return _currentTouch.position;}
#endif            
        }
        private Vector2 GetPointerPosition()
        {
            return PointerPosition;
        }

#if UNITY_EDITOR
        private Vector2 _pressMousePos;
#elif UNITY_ANDROID
        private Touch _currentTouch;
#endif

        private void Awake()
        {
            _touchHandlers      = new Dictionary<int, IPointerTouchHandler>();
            _downUpHandlers     = new Dictionary<int, IPointerDownUpHandler>();
            _enterExitHandlers  = new Dictionary<int, IPointerEnterExitHandler>();
            _localPointHandlers = new Dictionary<int, IPointerLocalPointHandler>();

            UIDelegatesContainer.GetEventsUpdater += GetUpdater;
        }
        private void OnDestroy()
        { 
            UIDelegatesContainer.GetEventsUpdater -= GetUpdater;
        }

        private UIPointerEventsUpdater GetUpdater()
        {
            return this;
        }

        public void AddPointerTouchHandler(IPointerTouchHandler handler)
        {
            _touchHandlers.Add(handler.InstanceID, handler);
        }
        public void AddPointerDownUpHandler(IPointerDownUpHandler handler)
        {
            _downUpHandlers.Add(handler.InstanceID, handler);
        }
        public void AddPointerEnterExitHandler(IPointerEnterExitHandler handler)
        {
            handler.EnterState = false;
            _enterExitHandlers.Add(handler.InstanceID, handler);
        }
        public void AddPointerLocalPointHandler(IPointerLocalPointHandler handler)
        {
            _localPointHandlers.Add(handler.InstanceID, handler);
        }

        public void RemovePointerTouchHandler(IPointerTouchHandler handler)
        {
            _touchHandlers.Remove(handler.InstanceID);
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
        public void RemovePointerLocalPointHandler(IPointerLocalPointHandler handler)
        {
            _localPointHandlers.Remove(handler.InstanceID);
        }

        public void RegisterMovingUI()
        {
            _movingUICount++;
        }
        public void UnregisterMovingUI()
        {
            _movingUICount--;
        }
        public void NotifyFinishedMove()
        {
            _finishedMovingUICount++;
            if (_finishedMovingUICount == _movingUICount)
            {
                UpdatePointerHandlers();
                _finishedMovingUICount = 0;
            }
        }

        public Vector2Int GetLocalPoint(RectTransform rect, out bool isValid)
        {
            isValid = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Input.touchCount != 1)
            {
                isValid = false;
                return Vector2Int.zero;
            }
#endif
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
                PointerPosition, null, out Vector2 localPoint
            );
            return new Vector2Int((int)localPoint.x, (int)localPoint.y);
        }

        private void Update()
        {
            if (_movingUICount == 0)
            {
                UpdatePointerHandlers();
                _finishedMovingUICount = 0;
            }
        }

#region UNITY_EDITOR
#if UNITY_EDITOR

        private void UpdatePointerHandlers()
        {
            UpdateDownUpHandlers();

            NotifyManyPointerExitIfNeeded();
            NotifyManyPointerEnterIfNeeded();
            NotifyLocalPointUpdateIfNeeded();

            if (Input.GetMouseButtonDown(0))
            {
                NotifyOnePointerTouchIfNeeded();
                return;
            }
        }

        private void UpdateDownUpHandlers()
        { 
            foreach (var pair in _downUpHandlers)
            {
                var handler = pair.Value;
                handler.IsPointerDown = Input.GetMouseButtonDown(0);
                handler.IsPointerUp = Input.GetMouseButtonUp(0);
            }
        }
#endif
#endregion

#region UNITY_ANDROID
#if UNITY_ANDROID && !UNITY_EDITOR
        private void UpdatePointerHandlers()
        {
            ResetDownUpHanlders();
            if (Input.touchCount != 1)
            {
                if (_touchDownUpState != TouchStateType.NoTouch)
                {
                    if (_touchDownUpState == TouchStateType.HasTouch)
                    {
                        _touchDownUpState = TouchStateType.ChangedToNoTouch;
                    }
                    else if (_touchDownUpState == TouchStateType.ChangedToNoTouch)
                    {
                        _touchDownUpState = TouchStateType.NoTouch;
                    }
                }
                OnNotOneTouch();
                return;
            }
            else
            {
                if (_touchDownUpState != TouchStateType.HasTouch)
                {
                    if (_touchDownUpState == TouchStateType.NoTouch)
                    {
                        _touchDownUpState = TouchStateType.ChangedToHasTouch;
                    }
                    else if (_touchDownUpState == TouchStateType.ChangedToHasTouch)
                    {
                        _touchDownUpState = TouchStateType.HasTouch;
                    }
                }
                OnOneTouch();
            }

            _currentTouch = Input.GetTouch(0);
    
            NotifyManyPointerExitIfNeeded();
            NotifyManyPointerEnterIfNeeded();
            NotifyLocalPointUpdateIfNeeded();

            if (_currentTouch.phase == TouchPhase.Began)
            {
                NotifyOnePointerTouchIfNeeded();
                return;
            }
        }

         private void OnNotOneTouch()
        { 
            foreach (var pair in _enterExitHandlers)
            {
                if (!pair.Value.ShouldInvokePointerEnterExitEvents)
                {
                    continue;
                }

                var handler = pair.Value;
                if (handler.EnterState)
                {
                    handler.EnterState = false;
                    handler.OnPointerExit();
                }
            }

            if (_touchDownUpState == TouchStateType.ChangedToNoTouch)
            { 
                foreach (var pair in _downUpHandlers)
                {
                    var handler = pair.Value;
                    handler.IsPointerUp = true;
                }
            }
        }

        private void OnOneTouch()
        {
            if (_touchDownUpState == TouchStateType.ChangedToHasTouch)
            {
                foreach (var pair in _downUpHandlers)
                {
                    var handler = pair.Value;
                    handler.IsPointerDown = true;
                }
            }
        }
        
        private void ResetDownUpHanlders()
        {
            foreach (var pair in _downUpHandlers)
            {
                var handler = pair.Value;
                handler.IsPointerDown = false;
                handler.IsPointerUp = false;
            }
        }
#endif
#endregion

        private void NotifyOnePointerTouchIfNeeded()
        { 
            foreach (var pair in _touchHandlers)
            {
                if (!pair.Value.ShouldInvokePointerTouchEvent)
                {
                    continue;
                }

                var handler = pair.Value;
                if (RectTransformUtility.RectangleContainsScreenPoint(handler.Rect, PointerPosition))
                { 
                    handler.OnPointerTouch();
                    return;
                }
            }
        }
        private void NotifyManyPointerExitIfNeeded()
        {
            foreach (var pair in _enterExitHandlers)
            {
                if (!pair.Value.ShouldInvokePointerEnterExitEvents)
                {
                    continue;
                }

                var handler = pair.Value;
                if (!RectTransformUtility.RectangleContainsScreenPoint(handler.InteractionRect, 
                    PointerPosition, null, Vector4.one * _offset))
                {
                    if (handler.EnterState)
                    {
                        handler.EnterState = false;
                        handler.OnPointerExit();
                    }
                }
            }
        }
        private void NotifyManyPointerEnterIfNeeded()
        { 
            foreach (var pair in _enterExitHandlers)
            {
                if (!pair.Value.ShouldInvokePointerEnterExitEvents)
                {
                    continue;
                }
                
                var handler = pair.Value;
                if (RectTransformUtility.RectangleContainsScreenPoint(handler.InteractionRect, 
                    PointerPosition, null, Vector4.one * _offset))
                {
                    if (!handler.EnterState)
                    {
                        handler.EnterState = true;
                        handler.OnPointerEnter();
                    }
                }
            }
        }
        private void NotifyLocalPointUpdateIfNeeded()
        { 
            foreach (var pair in _localPointHandlers)
            {
                var handler = pair.Value;
                if (handler.ShouldUpdateLocalPoint)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(handler.Rect,
                        PointerPosition, null, out Vector2 localPoint
                    );
                    handler.UpdateWithLocalPointFromPointer(new Vector2Int((int)localPoint.x, (int)localPoint.y));
                }
            }
        }
    }
}