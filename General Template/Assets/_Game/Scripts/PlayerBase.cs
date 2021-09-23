using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralTemplate;

public class PlayerBase : MonoBehaviour
{
    public virtual void ProcessGameEnd(GameResult result)
    {
        gameObject.SetActive(false);
    }
}
