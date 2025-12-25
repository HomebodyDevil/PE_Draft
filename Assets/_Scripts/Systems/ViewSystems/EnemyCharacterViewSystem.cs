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
        CreateEnemyCharacterView();
    }

    private void CreateEnemyCharacterView()
    {
        Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Views/Character/DefaultEnemyView.prefab").Completed +=
            handle =>
            {
                GameObject enemyViewPrefab = handle.Result;
                for (int i = 0; i < EnemySystem.Instance.EnemyCharacters.Count; i++)
                {
                    GameObject go = Instantiate(enemyViewPrefab, _enemyCharacterPositions[i]);
                    if (go.TryGetComponent<CharacterView>(out CharacterView characterView))
                    {
                        characterView.SetCharacter(EnemySystem.Instance.EnemyCharacters[i]);
                    }
                }
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
