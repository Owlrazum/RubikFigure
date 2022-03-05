using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : AnimatedPlayerCharacter
{
    [Header("Player")]
    [Space]
    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private float angularSpeed;

    private CharacterController characterController;
    private bool wasReactedToJoystickThisFrame;

    protected override void Awake()
    {
        base.Awake();
        characterController = GetComponent<CharacterController>();

        GeneralEventsContainer.JoystickCommanded += OnJoystickCommanded;
    }

    public void OnJoystickCommanded(JoystickCommand joy)
    {
        wasReactedToJoystickThisFrame = true; 

        float cameraEulerY = GeneralQueriesContainer.QueryCurrentCameraYaw();

        SetAnimationState(AnimationState.Running);

        Vector3 moveDirection = new Vector3(joy.Horiz, 0, joy.Vert);
        //moveDirection = Quaternion.Euler(0, cameraEulerY, 0) * moveDirection;
        
        characterController.Move(moveSpeed * Time.deltaTime * moveDirection);

        float rotateStep = angularSpeed * Time.deltaTime;
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation =
              Quaternion.RotateTowards(transform.rotation, targetRotation, rotateStep);
    }

    private void Update()
    {
        if (!wasReactedToJoystickThisFrame)
        {
            if (animationState != AnimationState.Idle)
            {
                SetAnimationState(AnimationState.Idle);
            }
        }
    }

    private void LateUpdate()
    {
        wasReactedToJoystickThisFrame = false;
    }
}

