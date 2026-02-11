using System;
using System.Collections.Generic;
using PEEnum;
using UnityEngine;

[Serializable]
public class TeamType
{
    [field: SerializeField] public Team Team { get; private set; }

    [field: SerializeField]
    public List<Classification> Classifications { get; private set; } = new();

    public TeamType() { }
    
    public TeamType(
        Team team, 
        Classification classification)
    {
        this.Team = team;
        this.Classifications.Add(classification);
    }
    
    public void SetTeam(Team team)
    {
        Debug.Log("Set Team");
        Team = team;
    }

    public void AddClassification(Classification classification)
    {
        Debug.Log("Set Classification");
        if (Classifications.Contains(classification))
        {
            Debug.Log("Classification already exists");
            return;
        }
        
        Classifications.Add(classification);
    }
    
    public void RemoveClassification(Classification classification)
    {
        Debug.Log("Set Classification");
        if (Classifications.Contains(classification))
        {
            Classifications.Remove(classification);
        }
    }
}
