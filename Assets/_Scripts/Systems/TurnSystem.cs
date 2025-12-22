using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : Singleton<TurnSystem>
{
    public Action<Character> OnCharacterStartTurn;
    
    private List<Character> _charactersTurnOrder = new();
    private int _currentTurnOrder = 0;
    
    private void Start()
    {
        SetInitialTurnOrder();
    }

    public void SetInitialTurnOrder()
    {
        _charactersTurnOrder.AddRange(PlayerSystem.Instance.PlayerCharacters);
        _charactersTurnOrder.AddRange(EnemySystem.Instance.EnemyCharacters);
    }

    public void NextTurn()
    {
        _currentTurnOrder = (_currentTurnOrder + 1) % _charactersTurnOrder.Count;
        OnCharacterStartTurn?.Invoke(_charactersTurnOrder[_currentTurnOrder]);
    }
}
