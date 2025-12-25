using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyCharacter _testEnemy;
    
    public List<Character> EnemyCharacters { get; private set; } = new();
    public List<Character> DeadEnemyCharacters { get; private set; } = new();

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
        EnemyCharacters.AddRange(EnemyService.Instance.EnemyCharacterList);
        Debug.Log($"EnemyCharacters Count: {EnemyCharacters.Count}");
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
}
