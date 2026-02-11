using System.Collections.Generic;
using PEEnum;
using UnityEngine;

public class WrappedPlayerCharacters : Character
{
    public List<Character> PlayerCharacters { get; private set; } = new();

    public WrappedPlayerCharacters() { }
    
    public WrappedPlayerCharacters(List<Character> characters)
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.Log("ERROR: WrappedPlayerCharacters cannot be null or empty.");
            return;
        }
        
        PlayerCharacters = characters;
        SetTeamType(new(Team.PlayerCharacter, Classification.Human));
    }
}
