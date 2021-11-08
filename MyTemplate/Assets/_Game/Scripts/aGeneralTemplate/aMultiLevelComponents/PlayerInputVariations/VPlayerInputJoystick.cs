using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    /// <summary>
    /// If needed, use contents of this class in player input controller.
    /// Non intended to use as it is.
    /// </summary>
    public class VPlayerInputJoystick : MonoBehaviour
    {
        [SerializeField]
        private FixedJoystick joystick;

        private void Update()
        {
            GameManager.Singleton.UpdatePlayerMovement(joystick.Horizontal, joystick.Vertical);
        }

        //public bool IsInteractingWithJoystick()
        //{
        //    return joystick.IsInteracting;
        //}

        //public void ProcessGameEnd()
        //{
        //    joystick.Reset();
        //}
    }
}

