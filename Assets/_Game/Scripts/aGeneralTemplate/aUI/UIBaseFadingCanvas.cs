using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(GraphicRaycaster))]
public class UIBaseFadingCanvas : MonoBehaviour
{
    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        graphicRaycaster = GetComponent<GraphicRaycaster>();

        fadeState = FadeState.Hided;
        graphicRaycaster.enabled = false;
    }

    [Header("FadingParameters")]
    [Space]
    [SerializeField]
    protected float fadeInTime = 0.5f;

    [SerializeField]
    protected float fadeOutTime = 0.5f;

    private enum FadeState
    {
        Shown,
        Hided,
        FadingIn,
        FadingOut
    }

    private FadeState fadeState;

    /// <summary>
    /// Should be used carefully, watch what is in the base class before using it in derived classes.
    /// </summary>
    protected CanvasGroup canvasGroup;
    
    private GraphicRaycaster graphicRaycaster;

    protected Action CompletedShowItself;
    protected Action CompletedHideItself;

    private IEnumerator fadingCoroutine;

    public virtual void ShowItself()
    {
        if (fadeState == FadeState.Hided)
        {
            fadingCoroutine = FadingIn();
            StartCoroutine(fadingCoroutine);
        }
        else if (fadeState == FadeState.FadingOut)
        {
            StopCoroutine(fadingCoroutine);
            fadingCoroutine = FadingIn();
            StartCoroutine(fadingCoroutine);
        }
    }

    private IEnumerator FadingIn()
    {
        fadeState = FadeState.FadingIn;
        float fadeParam = canvasGroup.alpha;
        while (fadeParam < 1)
        {
            fadeParam += Time.deltaTime / fadeInTime;
            canvasGroup.alpha = fadeParam;
            yield return null;
        }
        canvasGroup.alpha = 1;
        fadeState = FadeState.Shown;
        graphicRaycaster.enabled = true;
        CompletedShowItself?.Invoke();
    }   

    public virtual void HideItself()
    { 
        if (fadeState == FadeState.Shown)
        {
            fadingCoroutine = FadingOut();
            StartCoroutine(fadingCoroutine);
        }
        else if (fadeState == FadeState.FadingIn)
        {
            StopCoroutine(fadingCoroutine);
            fadingCoroutine = FadingOut();
            StartCoroutine(fadingCoroutine);
        }
    }

    private IEnumerator FadingOut()
    {
        fadeState = FadeState.FadingOut;
        float fadeParam = canvasGroup.alpha;
        while (fadeParam > 0)
        {
            fadeParam -= Time.deltaTime / fadeOutTime;
            canvasGroup.alpha = fadeParam;
            yield return null;
        }
        canvasGroup.alpha = 0;
        fadeState = FadeState.Hided;
        graphicRaycaster.enabled = false;
        CompletedHideItself?.Invoke();
    }
}
