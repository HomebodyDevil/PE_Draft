using UnityEngine;

public class PlayerReduceBlockGA : GameAbility
{
    [field: SerializeField] public bool ReduceAll { get; private set; }
    [field: SerializeField] public int ReduceAmount { get; private set; }
    [field: SerializeField] public string TargetCharacterName { get; private set; }
    
    public PlayerReduceBlockGA() { }

    public PlayerReduceBlockGA(bool reduceAll, int reduceAmount, PEEnum.PlayerCharacter targetCharacterType)
    {
        ReduceAll = reduceAll;
        ReduceAmount = reduceAmount;
        TargetCharacterName = targetCharacterType.ToString();
    }
    
    public PlayerReduceBlockGA(bool reduceAll, int reduceAmount, string targetCharacterName)
    {
        ReduceAll = reduceAll;
        ReduceAmount = reduceAmount;
        TargetCharacterName = targetCharacterName;
    }
}
