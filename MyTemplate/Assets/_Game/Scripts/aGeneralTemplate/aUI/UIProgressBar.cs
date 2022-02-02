using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

/// <summary>
/// Should be placed on separate canvas. Good practice.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class UIProgressBar : UIBaseFadingCanvas
{
    [Header("UIProgressBar")]
    [Space]
    [SerializeField]
    private Image imageToScale;

    private RectTransform progressBarTransform;

    private int sliceCountInLevel;
    private float scaleDelta;

    protected override void Awake()
    {
        base.Awake();
        progressBarTransform = imageToScale.rectTransform;
        
        GeneralEventsContainer.ProgressWasMade += OnProgressWasMade;

        CompletedHideItself += Reset;
    }

    public void Initialize(int sliceCountInLevelArg)
    {
        sliceCountInLevel = sliceCountInLevelArg;
        scaleDelta = 1.0f / sliceCountInLevel;
    }

    public void OnProgressWasMade()
    {
        float newScaleX = progressBarTransform.localScale.x +
            scaleDelta;
        progressBarTransform.DOScaleX(newScaleX, 1);
    }

    private void Reset()
    {
        progressBarTransform.localScale = new Vector3(0, 1, 1);
    }
}
