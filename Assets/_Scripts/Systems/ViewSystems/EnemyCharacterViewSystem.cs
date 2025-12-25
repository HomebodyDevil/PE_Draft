using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyCharacterViewSystem : Singleton<EnemyCharacterViewSystem>
{
    [SerializeField] private List<Transform> _enemyCharacterPositions = new();
    private List<CharacterView> _enemyCharacterViews = new();
    
    protected override void Awake()
    {
        base.Awake();
        SetVars();
    }

    private void OnEnable()
    {
        BattleEventSystem.Instance.OnCharacterDeath += DisableEnemyCharacterView;
    }

    private void OnDisable()
    {
        BattleEventSystem.Instance.OnCharacterDeath -= DisableEnemyCharacterView;
    }

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        CreateEnemyCharacterView();
    }
    
    private void EnableEnemyCharacterView(Character character)
    {
        foreach (var view in _enemyCharacterViews)
        {
            if (view.Character == character)
            {
                view.gameObject.SetActive(true);
            }
        }
    }

    private void DisableEnemyCharacterView(Character character)
    {
        foreach (var view in _enemyCharacterViews)
        {
            if (view.Character == character)
            {
                view.gameObject.SetActive(false);
            }
        }
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
                        _enemyCharacterViews.Add(characterView);
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
