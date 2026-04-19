using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    private void OnEnable()
    {
        GameAbilitySystem.Instance?.AddPerformer<DealDamageToTargetsGA>(DealDamageToTargetPerformer);
        GameAbilitySystem.Instance?.AddPerformer<DealDamageToRandomTargetsGA>(DealDamageToRandomTargetsPerformer);
        GameAbilitySystem.Instance?.AddPerformer<DealDamageToAllGA>(DealDamageToAllPerformer);
    }

    private void OnDisable()
    {
        GameAbilitySystem.Instance?.RemovePerformer<DealDamageToTargetsGA>();
        GameAbilitySystem.Instance?.RemovePerformer<DealDamageToRandomTargetsGA>();
        GameAbilitySystem.Instance?.RemovePerformer<DealDamageToAllGA>();
    }

    private void ReduceHealth(Character character, float reduceAmount)
    {
        if (character == null)
        {
            Debug.Log("No victim");
            return;
        }

        float newHealth = character.CurrentHealth - reduceAmount; 
        character.SetCurrentHealth(newHealth);
        Debug.LogError($"{character.CharacterName} New Health : {newHealth}");
    }

    private void CalcGiveDamageAmount(Character character, float baseDamage)
    {
        
    }
    
    private void CalcTakeDamageAmount(Character character, float baseDamage)
    {
        
    }

    public IEnumerator DealDamageToTargetPerformer(DealDamageToTargetsGA dealDamageToTargetsGA)
    {
        if (dealDamageToTargetsGA.Targets == null || dealDamageToTargetsGA.Targets.Count == 0)
        {
            Debug.Log("No Targets Found");
            yield break;
        }

        foreach (var target in dealDamageToTargetsGA.Targets)
        {
            if (target == null) continue;
            
            Debug.Log("단순히 BaseDamage를 입히는 중, 차후 수정해줘야 할 것.");
            ReduceHealth(target, dealDamageToTargetsGA.BaseDamage);
        }
            
        yield break;
    }

    public IEnumerator DealDamageToRandomTargetsPerformer(DealDamageToRandomTargetsGA dealDamageToRandomTargetsGA)
    {
        if (dealDamageToRandomTargetsGA == null || dealDamageToRandomTargetsGA.TargetCount == 0)
        {
            Debug.Log("No Targets");
            yield break;
        }

        Debug.Log("여기도 나중에 바꿀지 고민중.(지금은 적에게만 데미지 주고 있음)");
        int targetCnt = dealDamageToRandomTargetsGA.TargetCount;
        List<Character> targets = EnemySystem.Instance.EnemyCharacters.PickN(targetCnt);
        
        //Debug.Log($"Target Count {targets.Count}");

        if (targets == null || targets.Count == 0)
        {
            Debug.Log("No Targets Found");
            yield break;
        }
        
        foreach (var target in targets)
        {
            ReduceHealth(target, dealDamageToRandomTargetsGA.BaseDamage);
        }
    }

    public IEnumerator DealDamageToAllPerformer(DealDamageToAllGA dealDamageToAllGA)
    {
        var targetType = dealDamageToAllGA.TargetType;

        List<Character> targets = new();
        switch (targetType)
        {
            case PEEnum.TargetType.PlayerCharacter:
                targets = PlayerSystem.Instance.PlayerCharacters;
                break;
            case PEEnum.TargetType.Hostile:
                targets = EnemySystem.Instance.EnemyCharacters;
                break;
        }

        if (targets == null || targets.Count == 0)
        {
            Debug.Log("No Targets Found");
            yield break;
        }
        
        foreach (var target in targets)
            Debug.Log($"Target : {target.CharacterName}");
        
        foreach (var target in targets)
            ReduceHealth(target, dealDamageToAllGA.BaseDamage);
        
        yield break;
    }
}
