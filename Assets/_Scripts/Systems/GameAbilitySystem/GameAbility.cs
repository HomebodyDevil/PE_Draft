using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class GameAbility
{
    public abstract void ExecuteGameAbility(Character executor);
}
