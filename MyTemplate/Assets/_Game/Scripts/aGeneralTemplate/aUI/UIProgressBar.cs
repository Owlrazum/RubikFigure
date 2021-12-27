using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

/// <summary>
/// Should be place on separate canvas. Good practice.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class UIProgressBar : MonoBehaviour
{
    [SerializeField]
    private Image imageToScale;

    [SerializeField]
    private Image parentImage;

    private RectTransform progressBarTransform;
    private int sliceCountInLevel;
    private float scaleDelta;

    private bool isAppearing;
    private bool isDisappearing;

    private void Awake()
    {
        progressBarTransform = imageToScale.rectTransform;
    }

    public void Initialize(int sliceCountInLevelArg)
    {
        sliceCountInLevel = sliceCountInLevelArg;
        scaleDelta = 1.0f / sliceCountInLevel;
    }

    public void OnSliceFinished()
    {
        float newScaleX = progressBarTransform.localScale.x +
            scaleDelta;
        progressBarTransform.DOScaleX(newScaleX, 1);
    }

    Tween imageToScaleFadeTween;
    Tween parentImageFadeTween;

    public void Appear()
    {
        if (isDisappearing)
        {
            imageToScaleFadeTween.Complete();
            parentImageFadeTween.Complete();
        }
        isAppearing = true;
        gameObject.SetActive(true);
        imageToScaleFadeTween = imageToScale.DOFade(1, 2);
        parentImageFadeTween = parentImage.DOFade(1, 2).OnComplete(() =>
        {
            isAppearing = false;
        });
    }

    public void Dissapear()
    {
        if (isAppearing)
        {
            imageToScaleFadeTween.Complete();
            parentImageFadeTween.Complete();
        }
        isDisappearing = true;
        imageToScaleFadeTween = imageToScale.DOFade(0, 2);
        parentImageFadeTween = parentImage.DOFade(0, 2).OnComplete(() =>
        {
            gameObject.SetActive(false);
            isDisappearing = false;
            Reset();
        });
    }

    private void Reset()
    {
        progressBarTransform.localScale = new Vector3(0, 1, 1);
    }
}
