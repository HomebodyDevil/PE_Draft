using System;
using System.Collections.Generic;
using PEEnum;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class Character
{
    public string CharacterName { get; private set; } = "";
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public int Block;
    public TeamType TeamType { get; private set; }
    // Character들은 본인이 등록한 Reaction에 관한 리스트를 hold한다.
    // GameAbilitySystem의 AddReaction에서 추가해주고 있음.
    // public Dictionary<PEEnum.ReactionTiming, List<GameAbility>> AddedReactions { get; private set; } = new();
    public List<ReactionContext> Reactions { get; private set; } = new();

    private CharacterSkill characterSkill;
    public CharacterSkill CharacterSkill => characterSkill; 
    
    public Character() { }
    public Character(CharacterData characterData)
    {
        if (characterData == null)
        {
            Debug.LogError("CharacterData is null");
            return;
        }
        
        CharacterName = characterData.CharacterName;
        CurrentHealth = MaxHealth = characterData.MaxHealth;
        Block = 0;
        MaxHealth = characterData.MaxHealth;
        TeamType = characterData.TeamType;

        if (characterData.CharacterSkillData != null)
        {
            characterSkill = new(characterData.CharacterSkillData);
        }
        else
            Debug.LogError($"CharacterData.CharacterSkillData({characterData.CharacterName}) is null");
    }

    public void AddInitialReactions()
    {
        // 초기 Reaction을 추가하고자 할 때는 이것을 사용할 예정.
        Debug.Log("Adding reactions based on data : 초기 Reaction Setting");

        // foreach (var reaction in _initialReactions)
        // {
        //     GameAbilitySystem.Instance.AddReaction<StartCharacterTurnGA>(
        //         ReactionTiming.Pre,
        //         this,
        //         reaction,
        //         ReactionTarget.Player,
        //         -1234,
        //         false
        //     );
        // }
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
