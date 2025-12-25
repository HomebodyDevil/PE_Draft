using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Character
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public TeamType TeamType { get; private set; }
    // Character들은 본인이 등록한 Reaction에 관한 리스트를 hold한다.
    public Dictionary<PEEnum.ReactionTiming, List<GameAbility>> AddedReactions { get; private set; } = new();

    public Character() { }
    
    public Character(CharacterData characterData)
    {
        CurrentHealth = MaxHealth = characterData.MaxHealth;
        TeamType = characterData.TeamType;
    }

    public void SetTeamType(TeamType teamType)
    {
        TeamType = teamType;
    }

    public void SetCurrentHealth(float health)
    {
        CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);
        
        //Debug.Log($"current health: {CurrentHealth}");
        
        if (CurrentHealth == 0)
        {
            Debug.Log("zero health");

            CharacterDeathGA deathGA = new(this);
            GameAbilitySystem.Instance.RequestPerformGameAbility(this, new() {deathGA});
        }
    }

    public void AddAddedReaction(GameAbility reaction, PEEnum.ReactionTiming timing)
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

    public virtual void StartTurn()
    {
        Debug.Log("Character Start Turn");
    }
}
