using System.Collections.Generic;
using UnityEngine;

public class TargetGameAbility : GameAbility
{
    public List<Character> Targets { get; private set; }= new();
    [SerializeField] private List<string> targetTags = new();
    public IReadOnlyList<string> TargetTags => targetTags;

    public virtual void SetTargets(List<Character> targets)
    {
        Targets.Clear();
        Targets.AddRange(targets);
    }
    
    public virtual bool IsValidTarget(GameObject target)
    {
        if (target == null) return false;
        if (targetTags == null || targetTags.Count == 0) return true;

        foreach (var tag in targetTags)
        {
            if (target.CompareTag(tag))
                return true;
        }

        return false;
    }
}
