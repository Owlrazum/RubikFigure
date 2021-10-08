using UnityEngine;

namespace GeneralTemplate
{
    public class MoveMobile : MonoBehaviour
    {
        Vector3 directionOfMove;

        [SerializeField]
        private float moveSpeed;

        [SerializeField, Tooltip("Useful for not default orientations of camera")]
        private Vector3 rotationOffset;

        private void Awake()
        {
            initialForward = transform.forward;
            directionOfMove = transform.forward;
            rotationDirection = directionOfMove;
            shouldMove = true;
        }

        private bool shouldMove;
        public void StopMoving()
        {
            shouldMove = false;
        }

        public void ContinueMoving()
        {
            shouldMove = true;
        }

        float speedOfMove;
        public void UpdateMoveDirection(float moveX, float moveZ) // , bool isLocalSpace
        {
            directionOfMove = Quaternion.Euler(0, 0, 0) * (new Vector3(moveX, 0, moveZ)).normalized;
            directionOfMove = Quaternion.Euler(rotationOffset) * directionOfMove;
            speedOfMove = new Vector3(moveX, 0, moveZ).magnitude;
        }

        public void UpdateMoveDirection(Vector3 direction, float speedOfMoveArg)
        {
            directionOfMove = direction;
            directionOfMove = Quaternion.Euler(rotationOffset) * directionOfMove;
            speedOfMove = speedOfMoveArg;
        }

        public void ChangeMovementSpeed(float delta)
        {
            moveSpeed += delta;
        }

        private Vector3 initialForward;
        private Vector3 rotationDirection;
        bool isRotating = false;
        public void Move()
        {
            if (!shouldMove)
            {
                return;
            }
            Vector3 movement = directionOfMove * moveSpeed * speedOfMove * Time.deltaTime;
            transform.Translate(movement, Space.World);
            if (isRotating == false)
            {
                if (rotationDirection != directionOfMove)
                {
                    isRotating = true;
                    rotationDirection = directionOfMove;
                    RotateTo(initialForward, rotationDirection);
                }
            }
            else
            {
                RotateTo(initialForward, rotationDirection);
            }
        }

        private void RotateTo(Vector3 from, Vector3 to, bool isGradual = false)
        {
            Quaternion targetRotation = Quaternion.LookRotation(to, Vector3.up);
            if (isGradual)
            {
                transform.rotation =
                  Quaternion.RotateTowards(transform.rotation, targetRotation, 5);
            }
            else
            {
                transform.rotation = targetRotation;
            }

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1)
            {
                isRotating = false;
            }
        }
    }

}
