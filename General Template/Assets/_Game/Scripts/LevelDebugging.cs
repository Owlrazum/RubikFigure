using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using GeneralTemplate;

public class LevelDebugging : MonoBehaviour
{
    [SerializeField]
    private GameObject endGameUI;

    [SerializeField]
    private TextMeshProUGUI debugText;
    /// <summary>
    /// aka end level
    /// </summary>
    public void EndGame()
    {
        GameManager.Singleton.EndGame(GameResult.Win);
        endGameUI.SetActive(false);
        debugText.text = "Another " + debugText.text;
        debugText.fontSize -= 3;
    }
}
