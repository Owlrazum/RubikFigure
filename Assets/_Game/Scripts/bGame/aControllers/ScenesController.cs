using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{
    private AsyncOperation _loadingScene;

    private void Awake()
    {
        ApplicationDelegatesContainer.StartLoadingScene += StartLoadingScene;
        ApplicationDelegatesContainer.FinishLoadingScene += FinishLoadingScene;
        ApplicationDelegatesContainer.LoadMainMenu += LoadMainMenu;

        UIDelegatesContainer.GetSceneLoadingProgress += GetSceneLoadingProgress;
    }

    private void OnDestroy()
    { 
        ApplicationDelegatesContainer.StartLoadingScene -= StartLoadingScene;
        ApplicationDelegatesContainer.FinishLoadingScene -= FinishLoadingScene;
        ApplicationDelegatesContainer.LoadMainMenu -= LoadMainMenu;

        UIDelegatesContainer.GetSceneLoadingProgress -= GetSceneLoadingProgress;
    }

    private void StartLoadingScene(int sceneIndex)
    { 
        _loadingScene = SceneManager.LoadSceneAsync(sceneIndex);
        _loadingScene.allowSceneActivation = false;
        ApplicationDelegatesContainer.EventStartedLoadingScene?.Invoke();
    }

    private void FinishLoadingScene(Action callBack)
    { 
        _loadingScene.allowSceneActivation = true;
        StartCoroutine(CallBackCallOnLoadComplete(callBack));
    }

    private void LoadMainMenu(Action callBack)
    {
        _loadingScene = SceneManager.LoadSceneAsync(0);
        ApplicationDelegatesContainer.EventStartedLoadingScene?.Invoke();
        StartCoroutine(CallBackCallOnLoadComplete(callBack));
    }

    private IEnumerator CallBackCallOnLoadComplete(Action callBack)
    {
        while (!_loadingScene.isDone)
        {
            yield return null;
        }

        callBack?.Invoke();
    }

    private float GetSceneLoadingProgress()
    {
        if (_loadingScene != null)
        { 
            return _loadingScene.progress;
        }
        else
        {
            return -1;
        }
    }
}