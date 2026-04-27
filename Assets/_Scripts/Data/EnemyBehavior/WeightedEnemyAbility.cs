using System;
using UnityEngine;

[Serializable]
public class WeightedEnemyAbility
{
    [SerializeField] private CharacterSkillData skillData;
    //[SerializeReference, SR] private GameAbility ability;
    [SerializeField, Range(0, 100)] private int weight = 100;

    public CharacterSkillData CharacterSkillData => skillData;
    //public GameAbility Ability => ability;
    public int Weight => weight;
}
