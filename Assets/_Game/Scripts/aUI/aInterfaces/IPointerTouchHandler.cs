using UnityEngine;

namespace Orazum.UI
{ 
    public interface IPointerTouchHandler
    {
        public bool ShouldInvokePointerTouchEvent { get; }
        public void OnPointerTouch();
        public RectTransform Rect { get; }
        public int InstanceID { get; }
    }
}