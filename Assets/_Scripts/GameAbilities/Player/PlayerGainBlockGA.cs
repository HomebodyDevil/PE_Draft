using UnityEngine;

public class PlayerGainBlockGA : GameAbility
{
    [field: SerializeField] public int BlockAmount { get; private set; }
    [field: SerializeField] public PEEnum.PlayerCharacter TargetCharacter { get; private set; }

    public PlayerGainBlockGA() { }

    public PlayerGainBlockGA(int blockAmount, PEEnum.PlayerCharacter targetCharacter)
    {
        BlockAmount = blockAmount;
        TargetCharacter = targetCharacter;
    }
}
