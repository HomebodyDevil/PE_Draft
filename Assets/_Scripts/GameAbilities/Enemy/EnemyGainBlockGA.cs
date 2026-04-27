using UnityEngine;

public class EnemyGainBlockGA : GameAbility
{
    public EnemyCharacter TargetEnemy;
    public int BlockAmount;

    public EnemyGainBlockGA() { }

    public EnemyGainBlockGA(EnemyCharacter targetEnemy, int blockAmount)
    {
        TargetEnemy = targetEnemy;
        BlockAmount = blockAmount;
    }
}
