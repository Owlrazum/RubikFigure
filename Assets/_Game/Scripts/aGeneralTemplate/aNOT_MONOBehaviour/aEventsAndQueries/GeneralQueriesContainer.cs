using System;

using UnityEngine;

public static class GeneralQueriesContainer
{
    public static Func<int> ScenesCount;
    public static int QueryScenesCount()
    { 
#if UNITY_EDITOR
        if (ScenesCount.GetInvocationList().Length != 1)
        {
            throw new NotSupportedException("There should be only one subscription");
        }
#endif        

        return ScenesCount.Invoke();
    }


    public static Func<bool> AreAllLevelsPassed;
    public static bool QueryAreAllLevelsPassed()
    { 
#if UNITY_EDITOR
        if (AreAllLevelsPassed.GetInvocationList().Length != 1)
        {
            throw new NotSupportedException("There should be only one subscription");
        }
#endif        

        return AreAllLevelsPassed.Invoke();
    }

    public static Func<LevelData> LevelData;
    public static LevelData QueryLevelData()
    {
#if UNITY_EDITOR
        if (LevelData.GetInvocationList().Length != 1)
        {
            throw new NotSupportedException("There should be only one subscription");
        }
#endif        

        return LevelData.Invoke();
    }

    public static Func<bool> ShouldShowTutorial;
    public static bool QueryShouldShowTutorial()
    {
#if UNITY_EDITOR
        if (ShouldShowTutorial.GetInvocationList().Length != 1)
        {
            throw new NotSupportedException("There should be only one subscription");
        }
#endif        

        return ShouldShowTutorial.Invoke();
    }

    public static Func<float> CurrentCameraYaw;
    public static float QueryCurrentCameraYaw()
    { 
#if UNITY_EDITOR
        if (CurrentCameraYaw.GetInvocationList().Length != 1)
        {
            throw new NotSupportedException("There should be only one subscription");
        }
#endif

        return CurrentCameraYaw.Invoke();
    }

    public static Func<Vector3, Ray> CameraScreenPointToRay;
    public static Ray QueryCameraScreenPointToRay(Vector3 screenPos)
    { 
#if UNITY_EDITOR
        if (CameraScreenPointToRay.GetInvocationList().Length != 1)
        {
            throw new NotSupportedException("There should be only one subscription");
        }
#endif

        return CameraScreenPointToRay.Invoke(screenPos);
    }

    public static Func<Transform> CurrentPlayerTransform;
    public static Transform QueryCurrentPlayerTranfsorm()
    {
#if UNITY_EDITOR
        if (CurrentPlayerTransform.GetInvocationList().Length != 1)
        {
            throw new NotSupportedException("There should be only one subscription");
        }
#endif        

        return CurrentPlayerTransform.Invoke();
    }
}
