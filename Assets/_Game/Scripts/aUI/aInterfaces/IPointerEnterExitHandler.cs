using UnityEngine;

namespace Orazum.UI
{
    public interface IPointerEnterExitHandler
    {
        public void OnPointerEnter();
        public void OnPointerExit();
        public RectTransform InteractionRect { get; }
        public bool EnterState { get; set; }
        public int InstanceID { get; }
    }
}