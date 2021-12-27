using System.Collections.Generic;
using UnityEngine;

using TMPro;
using DG.Tweening;

namespace GeneralTemplate
{
    public class UIHowToPlayHint : MonoBehaviour
    {
        [SerializeField]
        private float timeOfFadingIn;

        [SerializeField]
        private float timeOfFadingOut;

        [SerializeField]
        private List<TextMeshProUGUI> hintTextComponents;

        private bool isHintShown;

        private Tween fadeTween;

        public void ShowHint()
        {
            if (isHintShown)
            {
                return;
            }

            foreach (var hintText in hintTextComponents)
            {
                fadeTween = DOTween.ToAlpha(() => hintText.color, x => hintText.color = x, 1, timeOfFadingIn).
                    SetEase(Ease.OutSine).OnComplete(() =>
                    {
                        isHintShown = true;
                    });
            }
        }

        public void HideHint()
        {
            if (!isHintShown)
            {
                fadeTween.Complete();
            }

            isHintShown = false;

            foreach (var hintText in hintTextComponents)
            {
                fadeTween = DOTween.ToAlpha(() => hintText.color, x => hintText.color = x, 0, timeOfFadingOut).
                    SetEase(Ease.InSine).OnComplete(() =>
                    {
                        isHintShown = false;
                    });
            }
        }
    }

}
