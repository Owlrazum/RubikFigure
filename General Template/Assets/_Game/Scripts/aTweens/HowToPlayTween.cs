using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class HowToPlayTween : MonoBehaviour
{
    [SerializeField]
    private float timeOfFading;
    void Start()
    {
        var texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            DOTween.ToAlpha(() => text.color, x => text.color = x, 1, timeOfFading).
                SetEase(Ease.OutSine).OnComplete(() =>
                {
                    DOTween.ToAlpha(() => text.color, x => text.color = x, 0, timeOfFading).SetEase(Ease.InSine);
                });
        }
    }
}
