using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace GeneralTemplate
{
    /// <summary>
    /// Should be parent of all CinemachineVirtualCameras;
    /// The index of virtual camera is equal to the sibling index,
    /// and equal to the camera location
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Camera renderingCamera;

        [Header("Shots")]
        [Space]
        [SerializeField]
        private CinemachineVirtualCamera firstShotCamera;

        [SerializeField]
        private CinemachineVirtualCamera secondShotCamera;

        [SerializeField]
        private CinemachineVirtualCamera thirdShotCamera;

        [System.Serializable]
        private enum CameraShotType
        {
            None,
            First,
            Second,
            Third
        }

        private CameraShotType currentCameraShot;

        private CinemachineBrain brain;

        private CinemachineVirtualCamera currentVcam;

        private void Awake()
        {
             brain = renderingCamera.GetComponent<CinemachineBrain>();

            firstShotCamera .Priority = 0;
            secondShotCamera.Priority = 0;
            thirdShotCamera .Priority = 0;

            currentVcam = firstShotCamera;
            renderingCamera.enabled = false;

            GeneralEventsContainer.LevelStart += OnLevelStart;

            GeneralQueriesContainer.CurrentCameraYaw += GetCameraYaw;
            GeneralQueriesContainer.CameraScreenPointToRay += GetCameraScreenPointToRay;
        }

        private void OnDestroy()
        { 
            GeneralEventsContainer.LevelStart -= OnLevelStart;

            GeneralQueriesContainer.CurrentCameraYaw += GetCameraYaw;
            GeneralQueriesContainer.CameraScreenPointToRay += GetCameraScreenPointToRay;
        }

        private void OnLevelStart(int notUsed)
        {
            ActivateCameraShot(CameraShotType.First);
            renderingCamera.enabled = true;
        }

        private void ActivateCameraShot(CameraShotType cameraShot)
        {
            if (currentCameraShot == cameraShot)
            {
                Debug.LogWarning("CameraController: The call for activation of camera shot is logically invalid.");
                return;
            }
            currentVcam.Priority = 0;
            switch (cameraShot)
            { 
                case CameraShotType.First:
                currentVcam = firstShotCamera;
                    break;
                case CameraShotType.Second:
                currentVcam = secondShotCamera;
                    break;
                case CameraShotType.Third:
                currentVcam = thirdShotCamera;
                    break;
            }
            currentVcam.Priority = 1;
        }

        private float GetCameraYaw()
        {
            return renderingCamera.transform.eulerAngles.y;
        }

         private Ray GetCameraScreenPointToRay(Vector3 screenPos)
        {
            return renderingCamera.ScreenPointToRay(screenPos);
        }
    }
}
