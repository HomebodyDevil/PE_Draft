using System;
using UnityEngine;

[Serializable]
public abstract class GameAbility
{
    [SerializeField] 
    private PEEnum.GAExecutor executor;

    public PEEnum.GAExecutor Executor => executor;

    public virtual void SetExecutor(PEEnum.GAExecutor executorType)
    {
        executor = executorType;
    }
}