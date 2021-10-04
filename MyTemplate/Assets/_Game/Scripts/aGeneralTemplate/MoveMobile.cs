using UnityEngine;

namespace GeneralTemplate
{
    public class MoveMobile : MonoBehaviour
    {
        Vector3 directionOfMove;

        [SerializeField]
        private float moveSpeed;

        //"Maybe useful at not default orientation of camera. Parent childs
        private float playerSpace;


        private void Start()
        {
            initialForward = transform.forward;
            directionOfMove = transform.forward;
            shouldMove = true;
        }


        float speedOfMove;
        public void UpdateMoveDirection(float moveX, float moveZ) // , bool isLocalSpace
        {
            directionOfMove = Quaternion.Euler(0, 0, 0) * (new Vector3(moveX, 0, moveZ)).normalized;
            speedOfMove = new Vector3(moveX, 0, moveZ).magnitude;
        }

        public void UpdateMoveDirection(Vector3 direction)
        {
            directionOfMove = direction;
        }

        public void ChangeMovementSpeed(float delta)
        {
            moveSpeed += delta;
        }

        private bool shouldMove;
        public void StopMoving()
        {
            shouldMove = false;
        }

        public void ResumeMoving()
        {
            shouldMove = true;
        }

        #region MoveProcessing

        private Vector3 initialForward;
        private Vector3 rotationDirection;
        bool isRotating = false;
        private void Move(Vector3 direction)
        {
            if (!shouldMove)
            {
                return;
            }
            Vector3 movement = direction * moveSpeed * speedOfMove * Time.deltaTime;
            Vector3 pos = transform.position + movement;
            transform.position = pos;
            //Debug.Log("Moved " + moveX + " " + moveZ);
            if (isRotating == false)
            {
                if (rotationDirection != direction)
                {
                    isRotating = true;
                    rotationDirection = direction;
                    RotateTo(initialForward, rotationDirection, true);
                }
            }
            else
            {
                RotateTo(initialForward, rotationDirection, true);
            }
        }
        private void RotateTo(Vector3 from, Vector3 to, bool isGradual = false)
        {
            float angle = Vector3.SignedAngle(from, to, transform.up);
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            if (isGradual)
            {
                //transform.rotation =
                //  Quaternion.RotateTowards(transform.rotation, targetRotation, 360);
            }
            else
            {
                //transform.rotation *= targetRotation;
            }

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1)
            {
                isRotating = false;
            }
        }
        #endregion
        private void Update()
        {
            Move(directionOfMove);
        }
    }

}
