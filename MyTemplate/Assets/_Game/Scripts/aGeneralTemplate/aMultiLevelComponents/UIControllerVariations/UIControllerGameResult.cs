using UnityEngine;
using System.Collections;

using TMPro;

namespace GeneralTemplate
{
    public class UIControllerGameResult : UIControllerBase
    {
        [SerializeField]
        private TextMeshProUGUI winText;

        [SerializeField]
        private TextMeshProUGUI defeatText;

        [SerializeField]
        private TextMeshProUGUI nextLevelText;

        [SerializeField]
        private TextMeshProUGUI repeatLevelText;

        public override void ProcessGameEnd(GameResult result)
        {
            endLevelCanvas.gameObject.SetActive(true);

            endLevelCanvasAnimator.Play("Appear");

            switch (result)
            {
                case GameResult.Win:
                    winText.gameObject.SetActive(true);
                    nextLevelText.gameObject.SetActive(true);
                    defeatText.gameObject.SetActive(false);
                    repeatLevelText.gameObject.SetActive(false);
                    break;
                case GameResult.Defeat:
                    defeatText.gameObject.SetActive(true);
                    repeatLevelText.gameObject.SetActive(true);
                    winText.gameObject.SetActive(false);
                    nextLevelText.gameObject.SetActive(false);
                    break;
            }
        }
    }
}
