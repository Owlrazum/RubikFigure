using UnityEngine;

public class AnimatorControllerRuntimeReferences : MonoBehaviour
{
    public static AnimatorControllerRuntimeReferences Singleton;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
    }

    public RuntimeAnimatorController sliceControlRes;
    public RuntimeAnimatorController generalControlRes;
}
