using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GeneralTemplate;
using System;

namespace GeneralTemplate
{
    [RequireComponent(typeof(MoveMobile))]
    [RequireComponent(typeof(AnimationComponent))]
    public class Player : MonoBehaviour
    {
        private MoveMobile movementComponent;
        private AnimationComponent animationComponent;

        private void Start()
        {
            movementComponent = GetComponent<MoveMobile>();
            animationComponent = GetComponent<AnimationComponent>();
            animationComponent.State = AnimationState.Idle;
        }

        public void ProcessGameEnd(GameResult result)
        {
            throw new NotImplementedException();
        }

        public void UpdateMovementInput(float inputX, float inputZ)
        {
            Vector3 inputVector = new Vector3(inputX, 0, inputZ);
            Vector3 direction = inputVector.normalized;

            float speedOfMove = inputVector.magnitude;

            //As second parameter speedOfMove can be used
            movementComponent.UpdateMoveDirection(direction, 1);  
            movementComponent.Move();

            if (speedOfMove > 0)
            {
                animationComponent.State = AnimationState.Walking;
            }
            else
            {
                animationComponent.State = AnimationState.Idle;
            }

            GameManager.Singleton.RotateAmorphs();
        }
    }
}

