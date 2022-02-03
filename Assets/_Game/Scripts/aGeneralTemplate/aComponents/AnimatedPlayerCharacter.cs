using UnityEngine;

public class AnimatedPlayerCharacter : MonoBehaviour
{
    [Header("AnimatedDancerCharacter")]
    [Space]
    [Tooltip("The animator for this class should be the first child in the hiearchy")]
    [SerializeField]
    private string readToolTip;

    private Animator animator;

    protected enum AnimationState
    {
        Idle,
        Walking,
        Running
    }

    protected AnimationState animationState;

    protected virtual void Awake()
    {
        animationState = AnimationState.Idle;
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    protected virtual void SetAnimationState(AnimationState newState)
    {
        if (animationState == newState)
        {
            return;
        }
        switch (newState)
        { 
            case AnimationState.Idle:
                animator.SetInteger("AnimationState", 0);
                break;
            case AnimationState.Walking:
                animator.SetInteger("AnimationState", 1);
                break;
            case AnimationState.Running:
                animator.SetInteger("AnimationState", 2);
                break;
        }
        animationState = newState;
    }
}