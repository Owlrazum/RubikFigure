using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    public class AnimationComponent : MonoBehaviour
    {
        private enum AnimationType
        {
            Default,
            BlendTree,
            Parameters
        }

        [Tooltip
        (
            "Default uses only one parameter int, " +
            "with no additional setup of this script. " +
            "BlendTree also may require setup of this script," +
            "with relatively small adjustments. " +
            "Parameters is for cases when multiple parameters in animator are needed." +
            "Requires additional setup in this script. " +
            "Specifically, State property in the 'set' part, 'switch' section"
        )]
        [SerializeField]
        private AnimationType type;

        private Animator animator;

        private void Awake()
        {
            animator = transform.GetChild(0).GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator, hence the model, " +
                    "should be in the first child of the player");
            }
        }

        private AnimationState state;

        /// <summary>
        /// Main 
        /// </summary>
        public AnimationState State
        {
            get
            {
                return State;
            }
            set
            {
                if (state == value)
                {
                    //Debug.Log("Already in the same animation state");
                    return;
                }
                switch (value)
                {
                    case AnimationState.Idle:
                        state = value;
                        if (type == AnimationType.Default)
                        {
                            animator.SetInteger("State", 0);
                        }
                        else if (type == AnimationType.Parameters)
                        {
                            // Additional setup goes here
                        }
                        else
                        {
                            if (!isDecreasingBlend)
                            {
                                StartCoroutine(DecreaseBlendTreeParameter());
                            }
                        }
                        break;
                    case AnimationState.Walking:
                        state = value;
                        if (type == AnimationType.Default)
                        {
                            animator.SetInteger("State", 1);
                        }
                        else if (type == AnimationType.Parameters)
                        {
                            // Additional setup goes here
                        }
                        else
                        {
                            // It is possible that you should change it
                            if (!isIncreasingBlend)
                            {
                                StartCoroutine(IncreaseBlendTreeParameter());
                            }
                        }
                        break;
                    case AnimationState.Running:
                        state = value;
                        if (type == AnimationType.Default)
                        {
                            animator.SetInteger("State", 2);
                        }
                        else if (type == AnimationType.Parameters)
                        {
                            // Additional setup goes here
                        }
                        break;
                }
            }
        }

        private bool isIncreasingBlend;
        private bool isDecreasingBlend;

        /// <summary>
        /// Transition from 0 to 1 happens in one second
        /// </summary>
        /// <returns></returns>
        private IEnumerator IncreaseBlendTreeParameter()
        {
            isIncreasingBlend = true;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime;
                animator.SetFloat("BlendTreeParameter", t);
                yield return null;
            }
            animator.SetFloat("BlendTreeParameter", 1);

            isIncreasingBlend = false;
        }

        /// <summary>
        /// Transition from 1 to 0 happens in one second
        /// </summary>
        /// <returns></returns>
        private IEnumerator DecreaseBlendTreeParameter()
        {
            isDecreasingBlend = true;

            float t = 1;
            while (t > 0)
            {
                t -= Time.deltaTime;
                animator.SetFloat("BlendTreeParameter", t);
                yield return null;
            }
            animator.SetFloat("BlendTreeParameter", 0);

            isDecreasingBlend = false;
        }
    }

    public enum AnimationState
    {
        Idle,
        Walking,
        Running
    }
}
