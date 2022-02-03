//using UnityEngine;

//namespace SDK_Integrations
//{
//    public class TenjinIntegration : MonoBehaviour
//    {
//        void Start()
//        {
//            TenjinConnect();
//        }

//        void OnApplicationPause(bool pauseStatus)
//        {
//            if (!pauseStatus)
//            {
//                TenjinConnect();
//            }
//        }

//        public void TenjinConnect()
//        {

//            // ------- ATTENTION!!! -------
//            // A specific API Key should be added!

//            BaseTenjin instance = Tenjin.getInstance("3F1TZPVMAKATNQFEJHB3NSD995ZA41QG");


//            // Sends install/open event to Tenjin
//            instance.Connect();
//        }
//    }
//}
