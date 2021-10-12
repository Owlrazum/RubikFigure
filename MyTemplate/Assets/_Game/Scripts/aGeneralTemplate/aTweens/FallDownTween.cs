using UnityEngine;

using DG.Tweening;

public class FallDownTween : MonoBehaviour
{
    [SerializeField]
    private float fallTime;

    [SerializeField]
    private float yPos;

    [SerializeField]
    private Ease ease;

    void Start()
    {
        transform.DOMoveY(yPos, fallTime).SetEase(ease).OnComplete(() =>
        {

        });
    }
}
