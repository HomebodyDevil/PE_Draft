using UnityEngine;

public class CharacterDeathGA : GameAbility
{
    public Character DeadCharacter { get; private set; }
    
    public CharacterDeathGA() { }

    public CharacterDeathGA(Character deadCharacter)
    {
        DeadCharacter = deadCharacter;
    }
}
