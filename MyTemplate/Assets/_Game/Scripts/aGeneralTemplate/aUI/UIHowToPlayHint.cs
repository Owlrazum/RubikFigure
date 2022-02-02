using System;

using UnityEngine;
using TMPro;

public class UIHowToPlayHint : UIBaseFadingCanvas
{
    protected override void Awake()
    {
        base.Awake();

        //EventsContainer.ShouldShowHint += ShowItself;
    }
}
