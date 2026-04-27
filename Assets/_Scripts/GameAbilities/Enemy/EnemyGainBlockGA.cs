using UnityEngine;

public class EnemyGainBlockGA : GameAbility
{
    public int BlockAmount;

    public EnemyGainBlockGA() { }

    public EnemyGainBlockGA(int blockAmount)
    {
        BlockAmount = blockAmount;
    }
}
