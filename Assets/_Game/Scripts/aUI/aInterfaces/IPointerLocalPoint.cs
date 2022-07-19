using UnityEngine;

namespace Orazum.UI
{ 
    public interface IPointerLocalPointHandler
    {
        public bool ShouldUpdateLocalPoint { get; }
        public RectTransform Rect { get; }
        public void UpdateLocalPoint(in Vector2Int localPointArg);
        public int InstanceID { get; }
    }
}