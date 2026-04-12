using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyTurnPattern
{
    [SerializeField] private List<WeightedEnemyAbility> behaviors = new();
    
    public List<WeightedEnemyAbility> Behaviors => behaviors;

    public GameAbility GetRandomAbility()
    {
        if (behaviors == null || behaviors.Count == 0)
        {
            Debug.LogWarning($"Behavior is invalid");
            return null;
        }

        int totalWeight = 0;
        foreach (var behavior in behaviors)
        {
            if (behavior == null) continue;
            totalWeight += behavior.Weight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning($"Total weight is less than 0");
            return null;
        }
        
        int roll = UnityEngine.Random.Range(0, totalWeight);
        int current = 0;

        foreach (var behavior in behaviors)
        {
            if (behavior == null || behavior.Ability == null) continue;
            
            current += Mathf.Max(0, behavior.Weight);
            if (roll < current)
                return behavior.Ability;
        }
        
        Debug.LogWarning($"Total weight is more than 0");
        return null;
    }
}
