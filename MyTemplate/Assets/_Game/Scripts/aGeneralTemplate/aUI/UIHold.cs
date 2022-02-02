using System.Collections;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reminds joystick, except it does not move anything, just fills up the inner space of the circle.
/// A small adjustment may be needed.
/// </summary>
public class UIHold : UIBaseFadingCanvas
{
    [Header("UIHold")]
    [Space]
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

    private float initialUnitformScale;
    private float currentUniformScale;

    private bool isReady;
    private bool isTouching;

    public void Initialize()
    {
        initialUnitformScale = innerCirlce.rectTransform.localScale.x;
        initialUnitformScale = 1;
        currentUniformScale = 1;
    }
    public void Reset()
    {
        currentUniformScale = initialUnitformScale;
        AssignScale(initialUnitformScale);
        isReady = false;
        isTouching = false;
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
}
