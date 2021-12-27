using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    /// <summary>
    /// To use it, choose which player input variation suits your project.
    /// Such class variations start with 'V', and are localed in the separate folder.
    /// By default, VPlayerInputJoystick is used.
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField]
        private FixedJoystick joystick;

        private void Update()
        {
            GameManager.Singleton.UpdatePlayerMovement(joystick.Horizontal, joystick.Vertical);
            if (Input.GetKeyDown(KeyCode.E))
            {
                //GameManager.Singleton.EndCurrentLevel();
            }
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
