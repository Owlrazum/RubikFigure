using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using GeneralTemplate;
using System;

namespace GeneralTemplate
{
    [RequireComponent(typeof(MoveMobile))]
    public class Player : MonoBehaviour
    {
        private MoveMobile movementComponent;

        private void Start()
        {
            movementComponent = GetComponent<MoveMobile>();
        }

        public void ProcessGameEnd(GameResult result)
        {
            throw new NotImplementedException();
        }

        public void UpdateMovementInput(float inputX, float inputZ)
        {
            movementComponent.UpdateMoveDirection(inputX, inputZ);
        }
    }
}

