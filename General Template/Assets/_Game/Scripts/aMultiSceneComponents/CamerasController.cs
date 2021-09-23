using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace GeneralTemplate
{
    public class CamerasController : MonoBehaviour
    {
        private Transform virtualCamerasParent;

        private CinemachineVirtualCamera[] virtualCameras;

        private int currentCamera;

        public void AssignCameras(Transform virtualCamerasParentArg)
        {
            virtualCamerasParent = virtualCamerasParentArg;

            virtualCameras = virtualCamerasParent.GetComponentsInChildren<CinemachineVirtualCamera>();
            foreach (var camera in virtualCameras)
            {
                camera.Priority = 0;
                print("check");
            }
            virtualCameras[0].Priority++;
            currentCamera = 0;
        }

        public void SwitchCamera(int first, int second)
        {
            if (currentCamera == first)
            {
                virtualCameras[first].Priority--;
                virtualCameras[second].Priority++;
                currentCamera = second;
            }
            else if (currentCamera == second)
            {
                virtualCameras[second].Priority--;
                virtualCameras[first].Priority++;
                currentCamera = first;
            }
            else
            {
                Debug.LogError("Invalid arguments for switching cameras");
            }
        }
    }
}
