using UnityEngine;

namespace GeneralTemplate
{
    public class UIControllerJoystick : UIControllerBase
    {
        [Header("JoystickCanvas")]
        [Space]
        [Space]
        [Space]
        [SerializeField]
        private Canvas joystickCanvas;

        public override void ProcessLevelEnd()
        {
            base.ProcessLevelEnd();

            joystickCanvas.gameObject.SetActive(false);
        }

        public override void ProcessNextLevelButtonDown()
        {
            base.ProcessNextLevelButtonDown();

            joystickCanvas.gameObject.SetActive(true);
        }
    }
}
