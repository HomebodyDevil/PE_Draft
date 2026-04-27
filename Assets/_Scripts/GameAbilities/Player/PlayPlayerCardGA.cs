using UnityEngine;

public class PlayPlayerCardGA : GameAbility
{
    public InBattleCard InBattleCardToPlay { get; set; }

    public PlayPlayerCardGA(InBattleCard inBattleCardToPlay)
    {
        InBattleCardToPlay = inBattleCardToPlay;
    }
}
