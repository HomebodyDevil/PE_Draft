using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="PlayerStatusData", menuName="Data/PlayerStatusData")]
public class PlayerStatusData : ScriptableObject
{
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public float MaxCost { get; private set; }
    [field: SerializeField] public List<CardData> Deck { get; private set; }
    [field: SerializeField] public List<CharacterData> CharactersData { get; private set; }
}
