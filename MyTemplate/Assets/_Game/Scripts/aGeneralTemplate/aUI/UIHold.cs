using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class UIHold : MonoBehaviour
{
    [SerializeField]
    private Image innerCirlce;

    [SerializeField]
    private Image outerCircle;

    [SerializeField]
    private UICustomPointer customPointer;

    [SerializeField]
    private float borderScaleValue;

    [SerializeField]
    private float speedOfFill;

    [SerializeField]
    private float speedOfFade;


    private enum FadeState
    {
        Appearing,
        Shown,
        Disappearing,
        Hided
    }

    private FadeState fadeState;
    private float fadeValue;

    private float initialUnitformScale;
    private float currentUniformScale;

    private bool isReady;
    private bool isTouching;

    public void Initialize()
    {
        initialUnitformScale = innerCirlce.rectTransform.localScale.x;
        initialUnitformScale = 1;
        currentUniformScale = 1;
        fadeValue = 0;
        //customPointer.Initialize(this);
    }
    public void Reset()
    {
        currentUniformScale = initialUnitformScale;
        AssignScale(initialUnitformScale);
        isReady = false;
        isTouching = false;
        fadeValue = 0;
    }

    public void ShowItself()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        StartCoroutine(AppearAnimation());
    }

    public void HideItself()
    {
        StartCoroutine(DisappearAnimation());
    }

    private IEnumerator AppearAnimation()
    {
        fadeState = FadeState.Appearing;
        while (true)
        {
            if (fadeState != FadeState.Appearing)
            {
                yield break;
            }
            fadeValue += speedOfFade * Time.deltaTime;
            if (fadeValue >= 1)
            {
                fadeState = FadeState.Shown;
                fadeValue = 1;
                AssignFade(fadeValue);
                yield break;
            }
            AssignFade(fadeValue);
            yield return null;
        }
    }

    private IEnumerator DisappearAnimation()
    {
        fadeState = FadeState.Disappearing;
        while (true)
        {
            if (fadeState != FadeState.Disappearing)
            {
                yield break;
            }
            fadeValue -= speedOfFade * Time.deltaTime;
            if (fadeValue <= 0)
            {
                fadeState = FadeState.Hided;
                fadeValue = 0;
                AssignFade(fadeValue);
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
                yield break;
            }
            AssignFade(fadeValue);
            yield return null;
        }
    }


    public void OnPointerDown()
    {
        isTouching = true;
    }

    public void OnPointerUp()
    {
        isTouching = false;
    }

    private void Update()
    {
        if (isTouching)
        {
            if (isReady)
            {
                // Send Event;
                //detailsTool.ProcessTouch();
            }
            else
            {
                currentUniformScale += speedOfFill * Time.deltaTime;
                if (currentUniformScale >= borderScaleValue)
                {
                    AssignScale(borderScaleValue);
                    currentUniformScale = borderScaleValue;
                    isReady = true;
                }
                else
                {
                    AssignScale(currentUniformScale);
                    isReady = false;
                }
            }
        }
        else
        {
            if (currentUniformScale > 0)
            {
                currentUniformScale -= speedOfFill * Time.deltaTime;
                if (currentUniformScale <= 0)
                {
                    currentUniformScale = 0;
                }
                AssignScale(currentUniformScale);
            }
            isReady = false;
        }
    }

    private void AssignScale(float scale)
    {
        Vector3 newScale = scale * Vector3.one;
        innerCirlce.transform.localScale = newScale;
    }

    private void AssignFade(float value)
    {
        Color newColor = new Color(1, 1, 1, value);
        innerCirlce.color = newColor;
        outerCircle.color = newColor;
    }
}
