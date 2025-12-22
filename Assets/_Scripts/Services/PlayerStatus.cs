using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStatus
{
    public float CurrentHealth { get; set; }
    public float MaxHealth { get; set; }
    public float MaxCost { get; set; }
    public List<Character> PlayerCharacters { get; set; } = new();
    public List<Card> PlayerDeck { get; private set; } = new();

    public PlayerStatus(
        PlayerStatusData data=null
    )
    {
        if (data == null)
        {
            CurrentHealth = ConstValue.DEFAULT_PLAYER_HEALTH;
            MaxHealth = ConstValue.DEFAULT_PLAYER_HEALTH;
            MaxCost = ConstValue.DEFAULT_PLAYER_COST;

            return;
        }
        
        CurrentHealth = data.MaxHealth;
        MaxHealth = data.MaxHealth;
        MaxCost = data.MaxCost;

        foreach (var characterData in data.CharactersData)
        {
            Character character = new(characterData);
            PlayerCharacters.Add(character);
        }

        foreach (var cardData in data.Deck)
        {
            Card card = new(cardData);
            PlayerDeck.Add(card);
        }
    }

    public void AddCardToPlayerDeck(Card newCard)
    {
        PlayerDeck.Add(newCard);
    }
}
