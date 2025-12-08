using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Data/CardData")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string CardName { get; private set; }
    [field: SerializeField, TextArea] public string CardDescription { get; private set; }
    [field: SerializeField] public int CardCost { get; private set; }
    [field: SerializeField] public ECardPlayType CardPlayType { get; private set; }
}
