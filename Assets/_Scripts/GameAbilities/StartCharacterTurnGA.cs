using UnityEngine;

public class StartCharacterTurnGA : GameAbility
{
    public Character TurnCharacter;

    public StartCharacterTurnGA() { }
    
    public StartCharacterTurnGA(Character character)
    {
        TurnCharacter = character;
        if (character is IEnemyTurnStart enemyTurnStart)
        {
            enemyTurnStart.TurnStart();
        }
        else
        {
            Debug.Log($"StartCharacter Turn : {character.CharacterName}");
        }
    }
}
