using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace GeneralTemplate
{
    public enum CameraLocation
    {
        Default,
        Custom
    }
    /// <summary>
    /// Should be parent of all CinemachineVirtualCameras;
    /// The index of virtual camera is equal to the sibling index,
    /// and equal to the camera location
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Camera renderingCamera;

        private CinemachineVirtualCamera[] virtualCameras;

        private CameraLocation currentCameraLocation;

        /// <summary>
        /// returns currentCameraLocation's virtualCamera index;
        /// </summary>
        /// <returns></returns>
        public int GetVCamIndexOfCurrentCameraLocation()
        {
            return (int)currentCameraLocation;
        }

        private void Awake()
        {
            virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>();
            foreach (var camera in virtualCameras)
            {
                camera.Priority = 0;
            }

            currentCameraLocation = CameraLocation.Default;
            virtualCameras[GetVCamIndexOfCurrentCameraLocation()].Priority++;

            QueriesContainer.CurrentCameraYaw += GetCameraYaw;

            virtualCameras[1].Priority = 1;
        }

        private void OnDisable()
        {
            QueriesContainer.CurrentCameraYaw -= GetCameraYaw;
        }

        private float GetCameraYaw()
        {
            return renderingCamera.transform.eulerAngles.y;
        }

        public void SwitchToCameraLocation(CameraLocation cameraLocation)
        {
            if (currentCameraLocation != cameraLocation)
            {
                virtualCameras[GetVCamIndexOfCurrentCameraLocation()].Priority--;
                currentCameraLocation = cameraLocation;
                virtualCameras[GetVCamIndexOfCurrentCameraLocation()].Priority++;
            }
        }

        public Ray GetScreenToWorldRay(Vector2 screenPos)
        {
            return renderingCamera.ScreenPointToRay(screenPos);
        }
    }
}
