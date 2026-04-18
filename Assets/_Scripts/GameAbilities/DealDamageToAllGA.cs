using PEEnum;
using UnityEngine;

public class DealDamageToAllGA : GameAbility
{
    [field: SerializeField] public float BaseDamage;
    public PEEnum.TargetType TargetType;

    public DealDamageToAllGA()
    {
        TargetType = TargetType.Hostile;
    }

    public DealDamageToAllGA(PEEnum.TargetType targetType)
    {
        TargetType = targetType;
    }
}
