using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(fileName="CharacterData", menuName="Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [field: SerializeField] public float MaxHealth { get; set; }
    [field: SerializeField] public float MaxCost { get; set; }
    [field: SerializeReference, SR] public TeamType TeamType { get; set; }
}
