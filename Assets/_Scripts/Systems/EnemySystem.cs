using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyCharacter _testEnemy;
    
    public List<Character> EnemyCharacters { get; private set; } = new();
    public List<Character> DeadEnemyCharacters { get; private set; } = new();

    private Coroutine _setEnemiesBasedOnMapNodeStatusCoroutine;
    
    private void OnEnable()
    {
        BattleEventSystem.Instance.OnCharacterDeath += MoveEnemyCharacterToDeadList;
    }

    private void OnDisable()
    {
        BattleEventSystem.Instance.OnCharacterDeath -= MoveEnemyCharacterToDeadList;
    }

    private void Start()
    {
        // EnemyCharacters.AddRange(EnemyService.Instance.EnemyCharacterList);
        // Debug.Log($"EnemyCharacters Count: {EnemyCharacters.Count}");
        
        EnemyCharacters.Clear();
        SetEnemiesBasedOnMapNodeStatus(PlayerStatusService.Instance.CurrentMapNodeStatus);
    }

    private void MoveEnemyCharacterToDeadList(Character character)
    {
        if (!EnemyCharacters.Contains(character))
        {
            Debug.Log("EnemyCharacter dont have character");
            return;
        }
        
        EnemyCharacters.Remove(character);
        DeadEnemyCharacters.Add(character);

        Debug.Log($"EnemyCharacters Count: {EnemyCharacters.Count}, DeadCount: {DeadEnemyCharacters.Count}");
    }

    public void RemoveEnemyInList(Character character)
    {
        if (character == null)
        {
            Debug.Log("Character to remove is null");
            return;
        }
        
        EnemyCharacters.Remove(character);
    }

    private void SetEnemiesBasedOnMapNodeStatus(MapNodeStatus mapNodeStatus = null)
    {
        if (_setEnemiesBasedOnMapNodeStatusCoroutine != null)
        {
            Debug.Log("SetEnemiesBasedOnMapNodeStatusCoroutine is running");
            StopCoroutine(_setEnemiesBasedOnMapNodeStatusCoroutine);
            _setEnemiesBasedOnMapNodeStatusCoroutine = null;
        }

        _setEnemiesBasedOnMapNodeStatusCoroutine = StartCoroutine(SetEnemiesBasedOnMapNodeStatusCoroutine(mapNodeStatus));
    }
    
    private IEnumerator SetEnemiesBasedOnMapNodeStatusCoroutine(MapNodeStatus mapNodeStatus = null)
    {
        mapNodeStatus ??= PlayerStatusService.Instance.CurrentMapNodeStatus;
        if (!mapNodeStatus.BattleEnemiesData.RuntimeKeyIsValid() || string.IsNullOrEmpty(mapNodeStatus.BattleEnemiesData.AssetGUID))
        {
            Debug.LogError("MapNodeStatus is Not Valid");
            _setEnemiesBasedOnMapNodeStatusCoroutine = null;
            yield break;
        }

        EnemyCharacters.Clear();
        
        AssetReferenceT<BattleEnemiesData> battleEnemiesDataRef = mapNodeStatus.BattleEnemiesData;
        var handle = battleEnemiesDataRef.LoadAssetAsync();

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.Log("Failed to load battle enemies");
            yield break;
        }

        BattleEnemiesData enemiesData = handle.Result;
        for (int i = 0; i < enemiesData.Enemies.Count; i++)
        {
            EnemyCharacter newEnemyCharacter = new(enemiesData.Enemies[i]);
            EnemyCharacters.Add(newEnemyCharacter);
            //newEnemyCharacter.PrintStatus();
        }
        // foreach (var enemyData in enemiesData.Enemies)
        // {
        //     EnemyCharacter newEnemyCharacter = new(enemyData);
        //     //newEnemyCharacter.PrintStatus();
        //     EnemyCharacterViewSystem.Instance.AddEnemyCharacterView(newEnemyCharacter);
        // }
        
        EnemyCharacterViewSystem.Instance.SetCharacterViewsBasedOnSystem(Team.Enemy);
        
        // Final
        //Addressables.Release(handle);
        _setEnemiesBasedOnMapNodeStatusCoroutine = null;
    }
}
