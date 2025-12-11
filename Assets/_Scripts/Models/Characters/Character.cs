using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }

    // Character들은 본인이 등록한 Reaction에 관한 리스트를 hold한다.
    public Dictionary<ReactionTiming, List<GameAbility>> AddedReactions { get; private set; } = new();
    
    public virtual void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        Debug.Log($"Character damaged. CurrentHealth = {CurrentHealth}");
        
        if (CurrentHealth <= 0)
        {
            Debug.Log("died");    
        }
        else
        {
            Debug.Log("Survived");
        }
        
        CurrentHealth = Mathf.Max(0f, CurrentHealth);
    }

    public void AddAddedReaction(GameAbility reaction, ReactionTiming timing)
    {
        if (AddedReactions.ContainsKey(timing))
        {
            AddedReactions[timing].Add(reaction);
        }
        else
        {
            AddedReactions.Add(timing, new List<GameAbility>() { reaction });
        }
    }
}
