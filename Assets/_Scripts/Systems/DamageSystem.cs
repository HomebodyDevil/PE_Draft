using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    private void OnEnable()
    {
        GameAbilitySystem.Instance?.AddPerformer<DealDamageToTargetsGA>(DealDamageToTargetPerformer);
        GameAbilitySystem.Instance?.AddPerformer<DealDamageToRandomTargetsGA>(DealDamageToRandomTargetsPerformer);
    }

    private void OnDisable()
    {
        GameAbilitySystem.Instance?.RemovePerformer<DealDamageToTargetsGA>();
        GameAbilitySystem.Instance?.RemovePerformer<DealDamageToRandomTargetsGA>();
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

        Debug.Log("여기도 나중에 바꿀 것.(지금은 적만 데미지 주고 있음)");
        int targetCnt = dealDamageToRandomTargetsGA.TargetCount;
        List<Character> targets = EnemySystem.Instance.EnemyCharacters.PickN(targetCnt);
        
        //Debug.Log($"Target Count {targets.Count}");

        foreach (var target in targets)
        {
            ReduceHealth(target, dealDamageToRandomTargetsGA.BaseDamage);
        }
    }
}
