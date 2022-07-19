// using System.Collections;

// using UnityEngine;
// using UnityEngine.UI;

// using Orazum.UI;

// /// <summary>
// /// Reminds joystick, except it does not move anything, just fills up the inner space of the circle.
// /// A small adjustment may be needed.
// /// </summary>
// public class UIHold : MonoBehaviour, IPointerTouchHandler
// {
//     [Header("UIHold")]
//     [Space]
//     [SerializeField]
//     private Image _innerCirlce;

//     [SerializeField]
//     private Image _outerCircle;

//     [SerializeField]
//     private float borderScaleValue;

//     [SerializeField]
//     private float speedOfFill;

//     private float initialUnitformScale;
//     private float currentUniformScale;

//     private bool isReady;
//     private bool isTouching;

//     public RectTransform Rect { get { return _outerCircle.rectTransform; } }
//     public int InstanceID { get { return GetInstanceID(); } }

//     private void Awake()
//     {
//         initialUnitformScale = _innerCirlce.rectTransform.localScale.x;
//         initialUnitformScale = 1;
//         currentUniformScale = 1;
//     }

//     private void Start()
//     {
//         UIEventsUpdater updater = UIDelegatesContainer.GetEventsUpdater();
//         updater.AddPointerTouchHandler(this);
//     }

//     public void OnPointerDown()
//     {
//         isTouching = true;
//     }

//     public void OnPointerUp()
//     {
//         isTouching = false;
//     }

//     private void Update()
//     {
//         if (isTouching)
//         {
//             if (isReady)
//             {
//                 // Send Event;
//             }
//             else
//             {
//                 currentUniformScale += speedOfFill * Time.deltaTime;
//                 if (currentUniformScale >= borderScaleValue)
//                 {
//                     AssignScale(borderScaleValue);
//                     currentUniformScale = borderScaleValue;
//                     isReady = true;
//                 }
//                 else
//                 {
//                     AssignScale(currentUniformScale);
//                     isReady = false;
//                 }
//             }
//         }
//         else
//         {
//             if (currentUniformScale > 0)
//             {
//                 currentUniformScale -= speedOfFill * Time.deltaTime;
//                 if (currentUniformScale <= 0)
//                 {
//                     currentUniformScale = 0;
//                 }
//                 AssignScale(currentUniformScale);
//             }
//             isReady = false;
//         }
//     }

//     private void AssignScale(float scale)
//     {
//         Vector3 newScale = scale * Vector3.one;
//         _innerCirlce.transform.localScale = newScale;
//     }

//     private void Reset()
//     {
//         currentUniformScale = initialUnitformScale;
//         AssignScale(initialUnitformScale);
//         isReady = false;
//         isTouching = false;
//     }
// }
