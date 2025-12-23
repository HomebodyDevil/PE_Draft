using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyService : PersistantSingleton<EnemyService>
{
    [SerializeField] private CharacterData _testEnemyCharacterData;

    [field: SerializeField] public List<Character> EnemyCharacterList { get; private set; } = new();
    
    private void Start()
    {
        Debug.Log("임시로 작업한 부분, 나중에 바꿔주자.");
        Character enemyChar = new(_testEnemyCharacterData);
        EnemyCharacterList.Add(enemyChar);
    }
}
