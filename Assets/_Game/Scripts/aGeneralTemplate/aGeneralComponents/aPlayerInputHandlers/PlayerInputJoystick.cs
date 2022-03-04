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

        private bool shouldSendJoy;

        private void Update()
        {
            if (joystick.Horizontal != 0 || joystick.Vertical != 0)
            {
                JoystickCommand command = new JoystickCommand(joystick.Horizontal, joystick.Vertical);
                GeneralEventsContainer.JoystickCommanded?.Invoke(command);
            }
        }
    }
}

