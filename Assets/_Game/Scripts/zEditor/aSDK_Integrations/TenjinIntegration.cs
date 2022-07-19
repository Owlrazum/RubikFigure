// using UnityEngine;

// namespace SDK_Integrations
// {
//     public class TenjinIntegration : MonoBehaviour
//     {
//         private void Awake()
//         {
//             DontDestroyOnLoad(gameObject);
//             GeneralEventsContainer.GameStart += OnGameStart;
//         }

//         private void OnDestroy()
//         {
//             GeneralEventsContainer.GameStart -= OnGameStart;
//         }

//         private void OnGameStart()
//         {
//             TenjinConnect();
//         }

//         private void OnApplicationPause(bool pauseStatus)
//         {
//             if (!pauseStatus)
//             {
//                 TenjinConnect();
//             }
//         }

//         public void TenjinConnect()
//         {

//             // ------- ATTENTION!!! -------
//             // A specific API Key should be added!

//             BaseTenjin instance = Tenjin.getInstance("FTNSVXSVBJYARJM4HPHM8X9PYIPDWO1V");


//             // Sends install/open event to Tenjin
//             instance.Connect();
//         }
//     }
// }
