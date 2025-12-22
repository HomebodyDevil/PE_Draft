using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private Character _testEnemy;
    
    public List<Character> EnemyCharacters { get; private set; } = new();
}
