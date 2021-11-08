using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GeneralTemplate
{
    /// <summary>
    /// If needed, use contents of this class in player input controller.
    /// To use it, add two methods at the bottom of the file to the GameManager.
    /// Not intended to use at it is.
    /// </summary>
    public class VPlayerInputOneTouch: MonoBehaviour
    {
        private bool isValidTouchStarted;
        private bool isTouching;

        private EventSystem eventSystem;
        private List<RaycastResult> raycastResults;

        private void Awake()
        {
            // Maybe in some universer there will be multiple eventSystems
            // and this code will break;
            eventSystem = EventSystem.current;
            raycastResults = new List<RaycastResult>();
        }

        private bool shouldLiftFinger;

        /// <summary>
        /// obviously, needs renaming for specific case.
        /// </summary>
        public void ProcessSomethingSoUserShouldLiftHisFinger()
        {
            shouldLiftFinger = true;
        }

        private void Update()
        {
#if UNITY_EDITOR
            EditorPlayerInput();
            return;
#endif

#if UNITY_ANDROID || UNITY_IOS
            BuildPlayerInput();
#endif
        }

        private void BuildPlayerInput()
        {
            if (Input.touchCount == 1)
            {
                Touch currentTouch = Input.GetTouch(0);

                if (currentTouch.phase == TouchPhase.Began)
                {
                    if (IsPointerOverUIObject())
                    {
                        isValidTouchStarted = false;
                        return;
                    }
                    else
                    {
                        isValidTouchStarted = true;
                    }
                }

                if (!isValidTouchStarted)
                {
                    return;
                }

                if (currentTouch.phase != TouchPhase.Canceled)
                {
                    if (shouldLiftFinger)
                    {
                        return;
                    }
                    isTouching = true;
                    //GameManager.Singleton.ProcessTouch();
                }
            }
            else
            {
                if (isTouching)
                {
                    isTouching = false;
                    isValidTouchStarted = false;
                    shouldLiftFinger = false;
                    //GameManager.Singleton.ProcessUntouch();
                }
            }
        }

        private bool IsPointerOverUIObject()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            raycastResults.Clear();
            eventSystem.RaycastAll(eventData, raycastResults);
            return raycastResults.Count > 0;
        }

        private void EditorPlayerInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    isValidTouchStarted = false;
                    return;
                }
                else
                {
                    isValidTouchStarted = true;
                }
            }

            if (!isValidTouchStarted)
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                if (shouldLiftFinger)
                {
                    return;
                }

                isTouching = true;

                //GameManager.Singleton.ProcessTouch();
            }
            else
            {
                if (isTouching)
                {
                    isTouching = false;
                    isValidTouchStarted = false;
                    shouldLiftFinger = false;
                    //GameManager.Singleton.ProcessUntouch();
                }
            }
        }
    }
}

// These methods are from Saw game, g100;

//public void ProcessTouch()
//{
//    if (isCuttableReady)
//    {
//        sawController.ProcessTouch();

//        MakeCuttingSawImpression();
//    }
//}

//public void ProcessUntouch()
//{
//    if (isCuttableReady)
//    {
//        sawController.ProcessUntouch();

//        MakeIdleSawImpression();
//    }
//}

