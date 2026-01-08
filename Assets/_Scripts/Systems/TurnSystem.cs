using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : Singleton<TurnSystem>
{
    public Action<Character> OnCharacterStartTurn;
    
    private List<Character> _charactersTurnOrder = new();
    private WrappedPlayerCharacters wrappedPlayerCharacters;
    
    private int _currentTurnOrder = 0;
    private bool _turnRequested = false;
    
    private void Start()
    {
        _currentTurnOrder = 0;
        SetInitialTurnOrder();
        
        StartCharacterTurnGA startTurnGA = new(_charactersTurnOrder[_currentTurnOrder]);
        GameAbilitySystem.Instance.RequestPerformGameAbility(
            _charactersTurnOrder[_currentTurnOrder],
            new() { startTurnGA });
    }

    private void OnEnable()
    {
        GameAbilitySystem.Instance?.AddPerformer<StartCharacterTurnGA>(StartCharacterTurnPerformer);
        GameAbilitySystem.Instance?.AddPerformer<EndCharacterTurnGA>(EndCharacterTurnPerformer);
    }

    private void OnDisable()
    {
        GameAbilitySystem.Instance?.RemovePerformer<StartCharacterTurnGA>();
        GameAbilitySystem.Instance?.RemovePerformer<EndCharacterTurnGA>();
    }

    public void SetInitialTurnOrder()
    {
        wrappedPlayerCharacters = new(PlayerSystem.Instance.PlayerCharacters);
        
        _charactersTurnOrder.Add(wrappedPlayerCharacters);
        _charactersTurnOrder.AddRange(EnemySystem.Instance.EnemyCharacters);
    }

    public void OnTurnButton()
    {
        Debug.Log("일단 시험삼아 TurnButton에 할당한 함수.");

        if (_charactersTurnOrder[_currentTurnOrder] != wrappedPlayerCharacters)
        {
            return;
        }

        if (_turnRequested)
        {
            return;
        }

        _turnRequested = true;
        EndCharacterTurnGA endTurnGA = new(_charactersTurnOrder[_currentTurnOrder]);
        GameAbilitySystem.Instance.RequestPerformGameAbility(
            _charactersTurnOrder[_currentTurnOrder],
            new() { endTurnGA });
    }

    public IEnumerator StartCharacterTurnPerformer(StartCharacterTurnGA startCharacterTurnGA)
    {
        Debug.Log($"Start Turn");

        if (startCharacterTurnGA == null)
        {
            Debug.Log("startCharacterTurnGA is null");
            yield break;
        }
        
        OnCharacterStartTurn?.Invoke(startCharacterTurnGA.TurnCharacter);
        
        Debug.Log($"teamType: {startCharacterTurnGA.TurnCharacter.TeamType.Team.ToString()}");

        if (startCharacterTurnGA.TurnCharacter.TeamType.Team == Team.PlayerCharacter)
        {
            Debug.Log("여기서 그냥 5장 드로우 함. 차후 수정할 필요 있음");

            _turnRequested = false;
            
            DrawCardsGA drawCardGA = new(5);
            GameAbilitySystem.Instance?.RequestPerformGameAbility(
                startCharacterTurnGA.TurnCharacter,
                new() { drawCardGA });
        }
        else
        {
            startCharacterTurnGA.TurnCharacter.StartTurn();
        }
        
        yield break;
    }

    public IEnumerator EndCharacterTurnPerformer(EndCharacterTurnGA endCharacterTurnGA)
    {
        _currentTurnOrder = (_currentTurnOrder + 1) % _charactersTurnOrder.Count;
        
        if (endCharacterTurnGA.TurnCharacter.TeamType.Team == Team.PlayerCharacter)
        {
            Debug.Log("여기서 그냥 DiscardPlayerCardsGA를 사용. 차후 수정할 필요 있어 보임.");

            DiscardPlayerCardsGA discardGA = new(true);
            GameAbilitySystem.Instance.RequestPerformGameAbility(
                endCharacterTurnGA.TurnCharacter,
                new() { discardGA });
        }
        
        StartCharacterTurnGA startTurnGA = new(_charactersTurnOrder[_currentTurnOrder]);
        GameAbilitySystem.Instance?.RequestPerformGameAbility(
            _charactersTurnOrder[_currentTurnOrder],
            new() { startTurnGA });
        
        yield break;
    }
}
