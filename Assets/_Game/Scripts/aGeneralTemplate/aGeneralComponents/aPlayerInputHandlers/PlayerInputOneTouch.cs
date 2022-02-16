using UnityEngine;
using UnityEngine.EventSystems;

namespace GeneralTemplate
{
    public class PlayerInputOneTouch : MonoBehaviour
    {
        private EventSystem eventSystem;

        private void Awake()
        {
            // Maybe in some universes there will be multiple eventSystems
            // and this code will break;
            eventSystem = EventSystem.current;
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

#if UNITY_EDITOR
        private void EditorPlayerInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                TouchCommand touch = new TouchCommand(Input.mousePosition);
                GeneralEventsContainer.TouchCommanded?.Invoke(touch);
            }
        }
#endif

#if UNITY_ANDROID || UNITY_IOS
        private void BuildPlayerInput()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    return;
                }
                TouchCommand touch = new TouchCommand(Input.GetTouch(0).position);
                GeneralEventsContainer.TouchCommanded?.Invoke(touch);
            }
        }
#endif
    }
}


        

