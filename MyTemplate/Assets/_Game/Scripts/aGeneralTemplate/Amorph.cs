using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace GeneralTemplate
{
    /// <summary>
    /// The class used for crowds or entities not hostile to the player.
    /// The content of the class serves as an example.
    /// </summary>
    public class Amorph : MonoBehaviour
    {
        [SerializeField, Tooltip("angle degrees in one second")]
        private float rotateSpeed;

        private Transform player;
        private float currentAngle;

        private bool isRotatingAround;

        public void StartRotatingAroundPlayer(Transform playerTransform, float startingAngleDegrees = 0)
        {
            print("roation");

            isRotatingAround = true;
            player = playerTransform;
            currentAngle = startingAngleDegrees;
            transform.RotateAround(player.position, Vector3.up, currentAngle);
        }

        public void UpdateRotateAround()
        {
            if (isRotatingAround)
            {
                currentAngle = rotateSpeed * Time.deltaTime;
                Quaternion rot = Quaternion.AngleAxis(currentAngle, Vector3.up);
                transform.position = rot * (transform.position - player.position) + player.position;
                transform.rotation = transform.rotation * rot;
            }
        }
    }
}
