using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Character
{
    public string CharacterName { get; private set; } = "";
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public TeamType TeamType { get; private set; }
    // Character들은 본인이 등록한 Reaction에 관한 리스트를 hold한다.
    public Dictionary<PEEnum.ReactionTiming, List<GameAbility>> AddedReactions { get; private set; } = new();
    public List<ReactionContext> Reactions { get; private set; } = new();

    public Character() { }
    
    public Character(CharacterData characterData)
    {
        CharacterName = characterData.CharacterName;
        CurrentHealth = MaxHealth = characterData.MaxHealth;
        MaxHealth = characterData.MaxHealth;
        TeamType = characterData.TeamType;
    }

    public void PrintStatus()
    {
        Debug.Log($"Name : {CharacterName}\nMax Health : {MaxHealth}\nTeamType : {TeamType.Team.ToString()}");
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
    
    public void AddAddedReaction(ReactionContext reactionContext)
    {
        Reactions.Add(reactionContext);
    }

    public void RemoveAddedReaction(ReactionContext reactionContext)
    {
        Reactions.Remove(reactionContext);
    }

    public virtual void StartTurn()
    {
        Debug.Log("Character Start Turn");
    }
}
