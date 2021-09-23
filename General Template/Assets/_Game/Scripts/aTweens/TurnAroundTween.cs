using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TurnAroundTween : MonoBehaviour
{
    [SerializeField]
    private float turnTime;
        
    void Start()
    {
        transform.DOLocalRotate(new Vector3(0, 120, 0), turnTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOLocalRotate(new Vector3(0, 240, 0), turnTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.DOLocalRotate(new Vector3(0, 360, 0), turnTime).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
            });
            
        });
    }

}
