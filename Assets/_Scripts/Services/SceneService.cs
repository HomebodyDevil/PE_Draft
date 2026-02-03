using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    BattleScene,
    TitleScene,
    MapScene,
    EventBattleScene,
    EventMapScene,
}

public class SceneService : PersistantSingleton<SceneService>
{
    [SerializeField] private float _minWaitTime = 1.5f;
    [SerializeField, Range(0.2f, 20f)] private float _fadeTime = 0.3f;
    [SerializeField] private Transform _loadingPanel;

    private Coroutine _fadeLoadingPanelCoroutine;
    private Coroutine _sceneLoadCoroutine;
    private List<string> _loadingScenes = new();

    protected override void Awake()
    {
        base.Awake();
        if (_loadingPanel == null) transform.AssignChildVar<Transform>("LoadingPanel", ref _loadingPanel);
    }

    private void Start()
    {
        _loadingPanel.gameObject.SetActive(false);
    }

    private static readonly Dictionary<SceneType, string> Scene = new()
    {
        { SceneType.BattleScene, "BattleScene" },
        { SceneType.TitleScene, "TitleScene" },
        { SceneType.MapScene, "MapScene" },
        { SceneType.EventBattleScene, "EventBattleScene" },
        { SceneType.EventMapScene, "EventMapScene" },
    };
    
    public void ChangeScene(SceneType sceneType)
    {
        string sceneName = GetSceneName(sceneType);
        FadeLoadingPanel(true);
        _sceneLoadCoroutine = StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (_loadingScenes.Contains(sceneName))
            yield break;
        
        _loadingScenes.Add(sceneName);
        float time = 0f;
        
        var loadSceneOP = SceneManager.LoadSceneAsync(sceneName);
        loadSceneOP.allowSceneActivation = false;

        while (!loadSceneOP.isDone)
        {
            time += Time.deltaTime;
            //Debug.Log($"load Scene Progress :  {loadSceneOP.progress * 100}%");

            if (time >= _minWaitTime)
            {
                // allowSceneActivation이 false면, isDone은 true로 바뀔 수 없음.
                loadSceneOP.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // race 걸릴 수도 있으니까 한 번 기다려주고.
        if (_fadeLoadingPanelCoroutine != null)
            yield return _fadeLoadingPanelCoroutine;
        FadeLoadingPanel(false);
        
        _loadingScenes.Remove(sceneName);
    }

    private string GetSceneName(SceneType scene)
    {
        if (!Scene.TryGetValue(scene, out var sceneName))
            throw new ArgumentOutOfRangeException(nameof(scene), scene, "Missing Scene");
        
        return sceneName;
    }

    private void FadeLoadingPanel(bool isIn)
    {
        Debug.Log("Fading LoadingPanel");
        
        _loadingPanel.gameObject.SetActive(true);
        
        if (isIn)
        {
            if (_fadeLoadingPanelCoroutine != null)
            {
                Debug.Log("FadeLoadingPanelCoroutine is running");
                StopCoroutine(_fadeLoadingPanelCoroutine);
                _fadeLoadingPanelCoroutine = null;
            }

            _fadeLoadingPanelCoroutine = StartCoroutine(FadeLoadingPanelCoroutine(true));
        }
        else
        {
            if (_fadeLoadingPanelCoroutine != null)
            {
                Debug.Log("FadeLoadingPanelCoroutine is running");
                StopCoroutine(_fadeLoadingPanelCoroutine);
                _fadeLoadingPanelCoroutine = null;
            }

            _fadeLoadingPanelCoroutine = StartCoroutine(FadeLoadingPanelCoroutine(false));
        }
    }

    private IEnumerator FadeLoadingPanelCoroutine(bool isIn)
    {
        Image panelImage = _loadingPanel.GetComponent<Image>();
        float time = 0f;
        float delta = 0f;

        float startAlpha = isIn ? 0f : 1f;
        float targetAlpha = isIn ? 1f : 0f;
        float currentAlpha;
        
        while (time <= _fadeTime)
        {
            time += Time.deltaTime;
            delta = time / _fadeTime;

            // if (delta >= 0.9f)
            //     currentAlpha = targetAlpha;
            // else
            //     currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, delta);
            
            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, delta);
            panelImage.color = new Color(panelImage.color.r, panelImage.color.g, panelImage.color.b, currentAlpha);
            
            yield return null;
        }

        if (!isIn) _loadingPanel.gameObject.SetActive(false);
        _fadeLoadingPanelCoroutine = null;
    }
}
