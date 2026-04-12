using System;
using SerializeReferenceEditor;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[Serializable]
public class WeightedEnemyAbility
{
    [SerializeReference, SR] private GameAbility ability;
    [SerializeField, Range(0, 100)] private int weight = 100;

    public GameAbility Ability => ability;
    public int Weight => weight;
}
