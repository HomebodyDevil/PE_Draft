using System;
using UnityEngine;

[Serializable]
public abstract class GameAbility
{
    private Character _caster;
    
    [SerializeField] 
    private PEEnum.GAExecutor executor;

    public Character Caster => _caster;
    
    public PEEnum.GAExecutor Executor => executor;

    public virtual void SetExecutor(PEEnum.GAExecutor executorType)
    {
        executor = executorType;
    }

    public void SetCaster(Character caster)
    {
        _caster = caster;
    }
}