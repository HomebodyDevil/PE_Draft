using System;
using System.Collections;
using UnityEngine;

public class GameEffectSystem : Singleton<GameEffectSystem>
{
    private void OnEnable()
    {
        GameAbilitySystem.Instance?.AddPerformer<DealDamageGA>(DealDamagePerformer);
    }

    private void OnDisable()
    {
        GameAbilitySystem.Instance?.RemovePerformer<DealDamageGA>();
    }

    public IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        yield break;
    }
}
