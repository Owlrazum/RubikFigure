using UnityEngine;

namespace Orazum.UI
{
    public interface IPointerDownUpHandler
    {
        public void OnPointerDown();
        public void OnPointerUp();
        public RectTransform InteractionRect { get; }
        public bool DownUpState { get; set; }
        public int InstanceID { get; }
    }
}