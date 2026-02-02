using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    private Coroutine _sceneLoadCoroutine;
    private List<string> _loadingScenes = new();

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
                loadSceneOP.allowSceneActivation = true;
            }

            yield return null;
        }

        _loadingScenes.Remove(sceneName);
    }

    private string GetSceneName(SceneType scene)
    {
        if (!Scene.TryGetValue(scene, out var sceneName))
            throw new ArgumentOutOfRangeException(nameof(scene), scene, "Missing Scene");
        
        return sceneName;
    }
}
