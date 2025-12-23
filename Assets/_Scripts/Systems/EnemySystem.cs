using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private Character _testEnemy;
    
    public List<Character> EnemyCharacters { get; private set; } = new();

    private void Start()
    {
        EnemyCharacters.AddRange(EnemyService.Instance.EnemyCharacterList);
        Debug.Log($"EnemyCharacters Count: {EnemyCharacters.Count}");
    }
}
