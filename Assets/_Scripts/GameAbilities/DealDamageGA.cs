using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DealDamageGA : GameAbility
{
    public override void ExecuteGameAbility(Character executor)
    {
        Debug.Log($"{executor.gameObject.name} execute dealing damage");
    }
}
