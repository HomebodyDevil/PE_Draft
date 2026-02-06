using UnityEngine;

public class CharacterDeathGA : GameAbility
{
    // BattleEventSystem에 Performer 구현.
    public Character DeadCharacter { get; private set; }
    
    public CharacterDeathGA() { }

    public CharacterDeathGA(Character deadCharacter)
    {
        DeadCharacter = deadCharacter;
    }
}
