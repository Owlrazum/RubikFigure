using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
public class UIBaseFadingCanvas : MonoBehaviour
{
    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        fadeState = FadeState.Hided;
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
    protected CanvasGroup _canvasGroup;
    
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
        float fadeParam = _canvasGroup.alpha;
        while (fadeParam < 1)
        {
            fadeParam += Time.deltaTime / fadeInTime;
            _canvasGroup.alpha = fadeParam;
            yield return null;
        }
        _canvasGroup.alpha = 1;
        fadeState = FadeState.Shown;
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
        float fadeParam = _canvasGroup.alpha;
        while (fadeParam > 0)
        {
            fadeParam -= Time.deltaTime / fadeOutTime;
            _canvasGroup.alpha = fadeParam;
            yield return null;
        }
        _canvasGroup.alpha = 0;
        fadeState = FadeState.Hided;
        CompletedHideItself?.Invoke();
    }
}
