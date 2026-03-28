using System.Collections.Generic;
using UnityEngine;

public class CharacterSkill
{
    public Sprite SkillImage { get; }
    public int SkillCost { get; }
    public string SkillName { get; }
    public string SkillDescription { get; }
    
    private List<GameAbility> skillAbilities = new();
    public IReadOnlyList<GameAbility> SkillAbilities => skillAbilities;

    public CharacterSkill(CharacterSkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogError("[CharacterSkill] skillData is null");
            skillAbilities = new List<GameAbility>();
            return;
        }
        
        SkillImage = skillData.SkillImage;
        SkillCost = skillData.SkillCost;
        SkillName = skillData.SkillName;
        SkillDescription = skillData.SkillDescription;
        skillAbilities = skillData.SkillAbilities != null
            ? new List<GameAbility>(skillData.SkillAbilities)
            : new List<GameAbility>();
        
        Debug.Log($"[CharacterSkill] skillData name: {skillData.name}");
        Debug.Log($"[CharacterSkill] SkillCost: {skillData.SkillCost}");
        Debug.Log($"[CharacterSkill] SkillName: {skillData.SkillName}");
        Debug.Log($"[CharacterSkill] SkillDescription: {skillData.SkillDescription}");
    }
}
