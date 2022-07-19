using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomMechanics
{
    /// <summary>
    /// Should have exactly two children placed at endpoints to properly initialize
    /// </summary>
    public class RepelSegment : MonoBehaviour
    {
        private Vector2 direction;
        private Vector2 normal;

        [TextArea]
        [SerializeField, Tooltip("Should have exactly two children.\n" +
            "They are used for computing direction of border.\n" +
            "Resulting vector should point to blue axis,\n" +
            "Bumping will happen to red axis." +
            "OnTriggerEnter should be modified to work")]
        private string notesForUsage;

        private void Awake()
        {
            if (transform.childCount != 2)
            {
                Debug.LogError("Not correct amount of children you have there. No more, no less)");
            }
            Vector3 start = transform.GetChild(0).position;
            Vector3 end = transform.GetChild(1).position;
            Initialize(end - start);
        }

        private void Start()
        {
            if (notesForUsage != "See Tooltip")
            {
                notesForUsage = "See Tooltip";
            }
        }

        private void Initialize(Vector3 directionArg)
        {
            direction = new Vector2(directionArg.x, directionArg.z);
            normal = Vector2.Perpendicular(direction);
        }
        private void OnTriggerEnter(Collider other)
        {
            // Code should be added here

            //Example:    )

            //if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            //{
            //    Player player = other.gameObject.GetComponentInParent<Player>();
            //    if (player != null)
            //    {
            //        player.MakeBumpedAndRepel(normal);
            //    }
            //}

            // At the end of the file code that was in the player in previous project.
        }
    }


}

//public void MakeBumpedAndRepel(Vector2 bumpNormalArg)
//{
//    isBumped = true;
//    bumpNormal = bumpNormalArg.normalized;
//}

//private IEnumerator RecoveringFromBump()
//{
//    isBumped = false;
//    isRecoveringFromBump = true;
//    yield return new WaitForSeconds(bumpRecoveryTime);
//    isRecoveringFromBump = false;
//    shouldRespondToJoystick = true;
//}


// Was placed in the update of the player

//if (isRecoveringFromBump)  // bump while recovering
//{
//    if (isBumped)
//    {
//        shouldRespondToJoystick = false;

//        Vector2 moveDirection = new Vector2(moveX, moveZ);
//        moveDirection = Vector2.Reflect(moveDirection, bumpNormal);

//        movementComponent.UpdateMoveControl(moveDirection.x, moveDirection.y);
//    }
//    return;
//}

//if (isBumped)
//{
//    shouldRespondToJoystick = false;

//    Vector2 moveDirection = new Vector2(moveX, moveZ);
//    moveDirection = Vector2.Reflect(moveDirection, bumpNormal);

//    movementComponent.UpdateMoveControl(moveDirection.x, moveDirection.y);

//    StartCoroutine(RecoveringFromBump());
//    return;
//}