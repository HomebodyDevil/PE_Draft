using System;
using UnityEngine;

public enum Team
{
    None,
    Player,
    Enemy,
}

// 종족(?)이라 생각하면 될듯.
public enum Classification
{
    None,
    Human,
}

[Serializable]
public class TeamType
{
    public Team Team { get; private set; }
    public Classification Classification { get; private set; }

    public void SetTeam(Team team)
    {
        Debug.Log("Set Team");
        Team = team;
    }

    public void SetClassification(Classification classification)
    {
        Debug.Log("Set Classification");
        Classification = classification;
    }
}
