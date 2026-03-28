using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(fileName="CharacterData", menuName="Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [field: SerializeField] public string CharacterName { get; set; } = ""; 
    [field: SerializeField] public float MaxHealth { get; set; }
    [field: SerializeField] public float MaxCost { get; set; }
    [field: SerializeReference, SR] public TeamType TeamType { get; set; }
    [field: SerializeReference, SR] public List<GameAbility> InitialReactions { get; set; } // 쓸지 안 쓸지 모르겠음.

    [field: SerializeField] public CharacterSkillData CharacterSkillData { get; set; }
}
