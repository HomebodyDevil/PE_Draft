using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DealDamageToTargetsGA : TargetGameAbility
{
    // DamageSystem에 Performer 구현.
    [field: SerializeField] public float BaseDamage { get; private set; }

    public DealDamageToTargetsGA() { }
    
    public DealDamageToTargetsGA(
        PEEnum.GAExecutor executor,
        float baseDamage)
    {
        
    }
}
