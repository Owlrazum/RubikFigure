using UnityEngine;

using TMPro;
using DG.Tweening;

public class LoopedFadingText : MonoBehaviour
{
    public void StartFading(float rateOfFading)
    {
        var texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            DOTween.ToAlpha(() => text.color, x => text.color = x, 1, rateOfFading).
                SetEase(Ease.OutSine).OnComplete(() =>
                {
                    DOTween.ToAlpha(() => text.color, x => text.color = x, 0, rateOfFading)
                    .SetEase(Ease.InSine).SetLoops(-1, LoopType.Yoyo);
                });
        }
    }

    public void StopFading()
    {
        DOTween.KillAll(true);
    }
}
