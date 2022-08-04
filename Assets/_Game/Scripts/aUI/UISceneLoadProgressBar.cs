using UnityEngine;
using UnityEngine.UI;

public class UISceneLoadProgressBar : MonoBehaviour
{
    [SerializeField]
    private Image _imageToScale;

    private void Awake()
    {
        _imageToScale.rectTransform.localScale = new Vector3(0, 1, 1);
    }

    private void Update()
    {
        float loadProgress = UIDelegatesContainer.GetSceneLoadingProgress();
        if (loadProgress < 0)
        {
            return;
        }

        loadProgress = Mathf.InverseLerp(0, 0.9f, loadProgress);
        _imageToScale.rectTransform.localScale = new Vector3(loadProgress, 1, 1);
    }
}
