using UnityEngine;

public class EnemyReduceBlockGA : GameAbility
{
    public int ReduceBlockAmount;

    public EnemyReduceBlockGA() { }

    public EnemyReduceBlockGA(int reduceBlockAmount)
    {
        ReduceBlockAmount = reduceBlockAmount;
    }
}
