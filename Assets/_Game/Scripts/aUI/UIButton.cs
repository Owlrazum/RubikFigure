using System;

using UnityEngine;
using UnityEngine.UI;
using Orazum.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UIButton : MonoBehaviour, IPointerTouchHandler, IPointerEnterExitHandler
{
    [SerializeField]
    private Color _highlightColor;

    private Color _defaultColor;

    private Image _image;

    public Action EventOnTouch { get; set; }
    public Action EventOnEnter { get; set; }
    public Action EventOnExit { get; set; }

    private RectTransform _rect;
    public RectTransform Rect { get { return _rect; } }
    public RectTransform InteractionRect { get { return _rect; } }

    public bool ShouldInvokePointerEnterExitEvents { get { return true; } }
    public bool ShouldInvokePointerTouchEvent { get { return true; } }

    private bool _enterState;
    public bool EnterState { get { return _enterState; } set { _enterState = value; } }

    public int InstanceID { get { return GetInstanceID(); } }

    private void Awake()
    {
        TryGetComponent(out _rect);
        TryGetComponent(out _image);

        _defaultColor = _image.color;
    }

    private void Start()
    {
        var uiUpdater = UIDelegatesContainer.GetEventsUpdater();
        uiUpdater.AddPointerTouchHandler(this);
        uiUpdater.AddPointerEnterExitHandler(this);
    }

    private void OnDestroy()
    {
        if (UIDelegatesContainer.GetEventsUpdater != null)
        { 
            var uiUpdater = UIDelegatesContainer.GetEventsUpdater();
            if (uiUpdater != null)
            { 
                uiUpdater.RemovePointerTouchHandler(this);
                uiUpdater.RemovePointerEnterExitHandler(this);
            }
        }
    }

    public virtual void OnPointerTouch()
    {
        EventOnTouch?.Invoke();
    }

    public virtual void OnPointerEnter()
    {
        _image.color = _highlightColor;
        EventOnEnter?.Invoke();
    }

    public virtual void OnPointerExit()
    {
        _image.color = _defaultColor;
        EventOnExit?.Invoke();
    }
}