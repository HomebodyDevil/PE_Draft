using UnityEngine;

public class DealDamageToRandomTargetsGA : GameAbility
{
    [field: SerializeField] public float BaseDamage { get; private set; }
    [field: SerializeField] public PEEnum.TargetType TargetType { get; private set; }
    [field: SerializeField] public int TargetCount { get; private set; }

    public DealDamageToRandomTargetsGA() { }
    
    public DealDamageToRandomTargetsGA(
        float baseDamage,
        int targetCount,
        PEEnum.TargetType targetType)
    {
        BaseDamage = baseDamage;
        TargetCount = targetCount;
        TargetType = targetType;
    }
}
