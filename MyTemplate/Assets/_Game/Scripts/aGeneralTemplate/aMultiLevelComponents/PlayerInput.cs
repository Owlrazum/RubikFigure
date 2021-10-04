using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    public class PlayerInput : MonoBehaviour
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

