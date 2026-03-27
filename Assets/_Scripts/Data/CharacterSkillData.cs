using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCharacterSkill",
    menuName = "Game/Skill/Character Skill",
    order = 0)]
public class CharacterSkillData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private Sprite skillImage;
    [SerializeField] private int skillCost;
    [SerializeField] private string skillName;

    [TextArea(3, 6)]
    [SerializeField] private string skillDescription;

    [Header("Abilities")]
    [SerializeReference] private List<GameAbility> skillAbilities = new List<GameAbility>();

    public Sprite SkillImage => skillImage;
    public int SkillCost => skillCost;
    public string SkillName => skillName;
    public string SkillDescription => skillDescription;
    public IReadOnlyList<GameAbility> SkillAbilities => skillAbilities;
}