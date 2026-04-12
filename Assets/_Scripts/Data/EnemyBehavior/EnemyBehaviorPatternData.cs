using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyBehaviourPattern",
    menuName = "Game/Enemy/Behaviour Pattern"
)]
public class EnemyBehaviorPatternData : ScriptableObject
{
    [SerializeField] private string patternDesc;
    [SerializeField] private bool loop = true;
    [SerializeField] private List<EnemyTurnPattern> turns = new();
    
    public string PatternDesc => patternDesc;
    public bool Loop => loop;
    public IReadOnlyList<EnemyTurnPattern> Turns => turns;

    public EnemyTurnPattern GetTurnPattern(int turnIndex)
    {
        if (turns == null || turns.Count == 0)
        {
            Debug.LogWarning($"No turn Data");
            return null;
        }

        turnIndex = Mathf.Max(turnIndex, 0);
        if (loop)
        {
            turnIndex %= turns.Count;
            return turns[turnIndex];
        }

        if (turnIndex >= turns.Count)
        {
            Debug.LogWarning($"No turn Data : turnIndex > turns.Count");
            return turns[^1];
        }
        
        return turns[turnIndex];
    }

    public GameAbility GetAbilityForTurn(int turnIndex)
    {
        var turnPattern = GetTurnPattern(turnIndex);
        if (turnPattern == null)
            return null;
        return turnPattern.GetRandomAbility();
    }
}
