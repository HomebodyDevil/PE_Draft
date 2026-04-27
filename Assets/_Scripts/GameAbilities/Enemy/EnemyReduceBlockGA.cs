using UnityEngine;

public class EnemyReduceBlockGA : GameAbility
{
    public EnemyCharacter TargetEnemy;
    public int ReduceBlockAmount;

    public EnemyReduceBlockGA() { }

    public EnemyReduceBlockGA(EnemyCharacter enemy, int reduceBlockAmount)
    {
        TargetEnemy = enemy;
        ReduceBlockAmount = reduceBlockAmount;
    }
}
