using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SerializeReferenceEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerStatusService : PersistantSingleton<PlayerStatusService>
{
    [field: SerializeField] public PlayerStatus PlayerStatus { get; private set; } = null;
    [field: SerializeField] public MapNodeStatus CurrentMapNodeStatus { get; set; } = null;
    [SerializeField] private PlayerStatusData defaultPlayerStatusData;

    public AsyncOperationHandle<PlayerStatusData> PlayerStatusDataHandle;
    
    private Coroutine _getSetDefaultPlayerStatusDataCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // PlayerStatus를 세팅.
        // 차후 저장된 데이터가 있다면, 그 데이터를 쓰도록 하자.
        GetSetDefaultPlayerStatusData();
    }

    private void SetupDefaultPlayerStatusData()
    {
        Debug.Log("여기서 defaultPlayerStatusData를 통해 기본 PlayerStatus를 구성함.");
        if (defaultPlayerStatusData != null)
            PlayerStatus = new(defaultPlayerStatusData);
        else
        {
            Debug.Log("PlayerStatusService : defaultPlayerData가 없음.");
            PlayerStatus = new PlayerStatus();
            PlayerStatus.MaxHealth = ConstValue.DEFAULT_PLAYER_HEALTH;
            PlayerStatus.CurrentHealth = ConstValue.DEFAULT_PLAYER_HEALTH;
            PlayerStatus.MaxCost = ConstValue.DEFAULT_PLAYER_COST;   
        }
    }

    private void GetSetDefaultPlayerStatusData()
    {
        if (_getSetDefaultPlayerStatusDataCoroutine != null)
        {
            Debug.Log("GetDefaultPlayerStatusData Coroutine is running");
            StopCoroutine(_getSetDefaultPlayerStatusDataCoroutine);
            _getSetDefaultPlayerStatusDataCoroutine = null;
        }
        
        _getSetDefaultPlayerStatusDataCoroutine = StartCoroutine(GetDefaultPlayerStatusDataCoroutine());
    }

    private IEnumerator GetDefaultPlayerStatusDataCoroutine()
    {
        var locHandle = Addressables.LoadResourceLocationsAsync(
            new List<object>() { "PlayerStatusData", "Default" },
            Addressables.MergeMode.Intersection,
            typeof(PlayerStatusData));

        yield return locHandle;

        if (locHandle.Status != AsyncOperationStatus.Succeeded ||
            locHandle.Result.Count == 0)
        {
            Debug.Log("Get Default Player Status Data Locations failed");
            yield break;
        }

        var dataLoc = locHandle.Result[0];
        var dataHandle = Addressables.LoadAssetAsync<PlayerStatusData>(dataLoc);

        yield return dataHandle;

        if (dataHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Get Default Player Status Data Failed");
            yield break;
        }
        
        defaultPlayerStatusData = dataHandle.Result;
        SetupDefaultPlayerStatusData();
        
        Addressables.Release(locHandle);
        Addressables.Release(dataHandle);

        _getSetDefaultPlayerStatusDataCoroutine = null;
        yield break;
    }

    public void LoadPlayerStatusData()
    {
        Debug.Log("Loading player status data: 차후 구현 바람.");
    }

    public List<Character> GetPlayerCharacters()
    {
        return PlayerStatus.PlayerCharacters;
    }
}
