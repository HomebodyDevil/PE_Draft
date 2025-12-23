using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyCharacterViewSystem : Singleton<EnemyCharacterViewSystem>
{
    [SerializeField] private List<Transform> _enemyCharacterPositions = new(); 
    
    protected override void Awake()
    {
        base.Awake();
        SetVars();
    }

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        CreateCharacterView();
    }

    private void CreateCharacterView()
    {
        Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Views/Character/DefaultEnemyView.prefab").Completed +=
            handle =>
            {
                GameObject enemyViewPrefab = handle.Result;
                Debug.Log("여기선 하나만 소환하는데, EnemyService의 데이터를 기반으로 데이터에 맞춰 생성토록 하자.");
                Instantiate(enemyViewPrefab, _enemyCharacterPositions[0]);
            };
    }

    private void SetVars()
    {
        Transform tr = null;
        if (transform.AssignChildVar<Transform>("EnemyCharacterPositions", ref tr))
        {
            _enemyCharacterPositions.Clear();
            tr.GetComponentsInChildren<Transform>(true, _enemyCharacterPositions);
            _enemyCharacterPositions.Remove(tr);
        }
    }
}
