using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAbility
{
    public float DamageAmount { get; private set; }

    public DealDamageGA(float damageAmount)
    {
        DamageAmount = damageAmount;
    }
}
