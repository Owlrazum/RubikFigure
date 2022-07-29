namespace Orazum.UI
{
    public interface IPointerDownUpHandler
    {
        public bool IsPointerDown { get; set; }
        public bool IsPointerUp { get; set; }
        public int InstanceID { get; }
    }
}