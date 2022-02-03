using UnityEngine;

namespace GeneralTemplate
{
    /// <summary>
    /// If additional info for the joystick command needed, modify the command itself and initialize it here.
    /// </summary>
    public class PlayerInputJoystick : MonoBehaviour
    {
        [SerializeField]
        private FixedJoystick joystick;

        private void Update()
        {
            JoystickCommand command = new JoystickCommand(joystick.Horizontal, joystick.Vertical);
            GeneralEventsContainer.InvokeJoystickCommanded(command);
        }
    }
}

