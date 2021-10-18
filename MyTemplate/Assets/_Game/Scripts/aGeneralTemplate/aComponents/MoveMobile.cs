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


        [Header("SlopedGround")]
        [Space]

        [SerializeField]
        [Tooltip("Turn on for global one direction slope for the movement. Look slopeOffset")]
        private bool isSloped = false;

        [SerializeField]
        [Tooltip("Does not work perfectly, should be adjusted a little bit")]
        private float slopeOffset = 0;

        [SerializeField]
        [Tooltip("forward vector of transform should not be sloped." +
            "It will show direction at which climb up or climb down " +
            "will happen")]
        private Transform slopeTransform;

        private Vector3 directionOfSlope;

        private void Awake()
        {
            initialForward = transform.forward;
            directionOfMove = transform.forward;
            rotationDirection = directionOfMove;

            if (isSloped)
            {
                directionOfSlope = slopeTransform.forward;
            }

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
            directionOfMove = new Vector3(moveX, 0, moveZ).normalized;
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
            if (isSloped)
            {
                Vector3 heightDeltaVector = Vector3.Project(directionOfMove, directionOfSlope);
                float signOfHeightFactor = CustomMath.GetDotProdSign(directionOfMove, directionOfSlope);
                float scaleOfHeightFactor = heightDeltaVector.magnitude;

                float sinPart = Mathf.Sin(slopeOffset * Mathf.Deg2Rad);
                float cosPart = Mathf.Cos(slopeOffset * Mathf.Deg2Rad);

                directionOfMove *= cosPart; // scale without heighFactor;

                float heightFactor = signOfHeightFactor * scaleOfHeightFactor * sinPart;

                directionOfMove.y = heightFactor;
                directionOfMove.Normalize();
            }
            Vector3 movement = directionOfMove * moveSpeed * speedOfMove * Time.deltaTime;
            Vector3 endPosition = transform.position + movement;
            transform.position = endPosition;
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
