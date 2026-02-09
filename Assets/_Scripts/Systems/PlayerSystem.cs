using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : Singleton<PlayerSystem>
{
    [SerializeField] private bool _forTest = false;
    [SerializeField] private Character _testPlayer;
    
    [field: SerializeField] public List<Character> PlayerCharacters { get; private set; } = new();

    private void Start()
    {
        InitPlayerCharacters();
    }

    public void InitPlayerCharacters()
    {
        if (_forTest && _testPlayer != null)
        {
            PlayerCharacters.Add(_testPlayer);
            return;
        }
        
        PlayerCharacters.AddRange(PlayerStatusService.Instance.GetPlayerCharacters());
        PlayerCharacterViewSystem.Instance.SetCharacterViewsBasedOnSystem(Team.PlayerCharacter);

        // foreach (var playerCharacter in PlayerCharacters)
        // {
        //     playerCharacter.AddInitialReactions();
        // }

        TurnSystem.Instance.PlayerReady = true;
    }

    public Character FindPlayerCharacter(PEEnum.PlayerCharacter playerCharacter)
    {
        string characterName = playerCharacter.ToString();
        foreach (var character in PlayerCharacters)
        {
            if (string.Equals(character.CharacterName, characterName))
            {
                return character;
            }
        }

        return null;
    }
    
    public Character FindPlayerCharacter(string playerCharacterName)
    {
        foreach (var character in PlayerCharacters)
        {
            if (string.Equals(character.CharacterName, playerCharacterName))
            {
                return character;
            }
        }

        return null;
    }
}
