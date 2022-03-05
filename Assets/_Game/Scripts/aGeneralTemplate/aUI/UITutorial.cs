using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorial : UIBaseFadingCanvas
{
    [Header("UITutorial")]
    [Space]
    [SerializeField]
    private RectTransform finger;

    [SerializeField]
    private float deltaAngle = 10;

    [SerializeField]
    private float timeOfCycle = 0.5f;

    [SerializeField]
    private int cycleCount = 2;

    protected override void Awake()
    {
        base.Awake();

        //EventsContainer.BeforePlayerCameraShotActive += OnBeforePlayerCameraShotActive;
    }

    private void OnDestroy()
    { 
        //EventsContainer.BeforePlayerCameraShotActive -= OnBeforePlayerCameraShotActive;
    }

    private void OnBeforePlayerCameraShotActive()
    {
        if (GeneralQueriesContainer.QueryShouldShowTutorial())
        {
            StartCoroutine(ShowTutorialCoroutine());
        }
        else
        {
            //EventsContainer.PlayerCameraShotActive?.Invoke();
        }
    }

    private IEnumerator ShowTutorialCoroutine()
    {
        ShowItself();
        yield return new WaitForSeconds(fadeInTime - Time.deltaTime);
        float initialRoll = finger.eulerAngles.z;
        float targetRoll = initialRoll + deltaAngle;
        for (int i = 0; i < cycleCount; i++)
        { 
            float lerpParam = 0;
            while (lerpParam < 1)
            {
                lerpParam += Time.deltaTime / (timeOfCycle / 2);
                float currentRoll = Mathf.Lerp(initialRoll, targetRoll, CustomMath.EaseOut(lerpParam));
                finger.rotation = Quaternion.Euler(Vector3.forward * currentRoll);
                yield return null;
            }

            lerpParam = 0;
            while (lerpParam < 1)
            { 
                lerpParam += Time.deltaTime / (timeOfCycle / 2);
                float currentRoll = Mathf.Lerp(targetRoll, initialRoll, CustomMath.EaseIn(lerpParam));
                finger.rotation = Quaternion.Euler(Vector3.forward * currentRoll);
                yield return null;
            }
        }
        HideItself();
        yield return new WaitForSeconds(fadeOutTime -  Time.deltaTime);
        print("Calling event");
        InvokePlayerCameraShotActive();
    }

    private void InvokePlayerCameraShotActive()
    { 
        //EventsContainer.PlayerCameraShotActive?.Invoke();
    }
}
