using System;
using System.Collections;
using System.Collections.Generic;
using PEEnum;
using UnityEngine;

public class TurnSystem : Singleton<TurnSystem>
{
    public Action<Character> OnCharacterStartTurn;

    private List<Character> _charactersTurnOrder = new();
    private WrappedPlayerCharacters wrappedPlayerCharacters;

    private int _currentTurnOrder = 0;
    private bool _turnRequested = false;
    public bool PlayerReady = false;
    public bool EnemiesReady = false;
    public bool TurnReady = false;

    private void Start()
    {
        //_currentTurnOrder = 0;
        //SetInitialTurnOrder();

        // StartCharacterTurnGA startTurnGA = new(_charactersTurnOrder[_currentTurnOrder]);
        // GameAbilitySystem.Instance.RequestPerformGameAbility(
        //     _charactersTurnOrder[_currentTurnOrder],
        //     new() { startTurnGA });

        InitializeTurn();
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

    private Coroutine _initializeTurnCoroutine;

    private void InitializeTurn()
    {
        if (_initializeTurnCoroutine != null)
        {
            Debug.Log("InitializeTurnCoroutine is running");
            StopCoroutine(_initializeTurnCoroutine);
            _initializeTurnCoroutine = null;
        }

        _initializeTurnCoroutine = StartCoroutine(InitializeTurnCoroutine());
    }

    private IEnumerator InitializeTurnCoroutine()
    {
        int loopCnt = 0;
        while (!PlayerReady || !EnemiesReady)
        {
            if (loopCnt > ConstValue.MAX_LOOP)
            {
                Debug.Log("Too much loop");
                yield break;
            }

            yield return new WaitForSeconds(0.005f);
        }

        _charactersTurnOrder.Clear();
        wrappedPlayerCharacters = new(PlayerSystem.Instance.PlayerCharacters);
        _charactersTurnOrder.Add(wrappedPlayerCharacters);
        _charactersTurnOrder.AddRange(EnemySystem.Instance.EnemyCharacters);

        _currentTurnOrder = 0;
        StartCharacterTurnGA startTurnGA = new(_charactersTurnOrder[_currentTurnOrder]);
        GameAbilitySystem.Instance.RequestPerformGameAbility(
            _charactersTurnOrder[_currentTurnOrder],
            new() { startTurnGA });

        TurnReady = true;

        _initializeTurnCoroutine = null;
    }

    // public void SetInitialTurnOrder()
    // {
    //     wrappedPlayerCharacters = new(PlayerSystem.Instance.PlayerCharacters);
    //     
    //     _charactersTurnOrder.Add(wrappedPlayerCharacters);
    //     _charactersTurnOrder.AddRange(EnemySystem.Instance.EnemyCharacters);
    //     
    //     Debug.Log($"Characters Count in Turn System: {_charactersTurnOrder.Count}");
    //     foreach (var character in _charactersTurnOrder)
    //         Debug.Log($"Character in Turn System : {character.CharacterName}");
    // }

    public void AddCharactersToTurnList(List<Character> characters)
    {
        _charactersTurnOrder.AddRange(characters);
        if (_charactersTurnOrder.Count == 0)
        {
            Debug.Log("No Characters Turn Order");
            return;
        }

        if (_charactersTurnOrder[0] != wrappedPlayerCharacters)
        {
            _charactersTurnOrder.Remove(wrappedPlayerCharacters);
            _charactersTurnOrder.Insert(0, wrappedPlayerCharacters);
        }
    }

    public void RemoveCharacterInTurnList(Character character)
    {
        if (!_charactersTurnOrder.Contains(character))
            Debug.Log($"Theres no {character.CharacterName} in Turn list");

        _charactersTurnOrder.Remove(character);
    }

    public void OnTurnButton()
    {
        Debug.Log("일단 시험삼아 TurnButton에 할당한 함수.");

        if (!TurnReady)
        {
            Debug.Log("Turn is not ready yet.");
            return;
        }

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

            foreach (var playerCharacter in PlayerSystem.Instance.PlayerCharacters)
            {
                if (playerCharacter == null)
                {
                    Debug.LogError("playerCharacter is null");
                    continue;
                }

                PlayerReduceBlockGA reduceBlockGA = new(
                    true,
                    0,
                    playerCharacter.CharacterName);
                
                GameAbilitySystem.Instance.RequestPerformGameAbility(playerCharacter, new() { reduceBlockGA });
            }
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
            Debug.Log("EndCharacterTurnPerformer에서 그냥 DiscardPlayerCardsGA를 사용. 차후 수정할 필요 있어 보임.");

            DiscardPlayerCardsGA discardGA = new(true);
            GameAbilitySystem.Instance.RequestPerformGameAbility(
                endCharacterTurnGA.TurnCharacter,
                new() { discardGA });
        }

        if (endCharacterTurnGA.TurnCharacter == wrappedPlayerCharacters)
            Debug.Log("End Turn : Player");
        else
            Debug.Log($"End Turn : {endCharacterTurnGA.TurnCharacter.CharacterName}");

        StartCharacterTurnGA startTurnGA = new(_charactersTurnOrder[_currentTurnOrder]);
        GameAbilitySystem.Instance?.RequestPerformGameAbility(
            _charactersTurnOrder[_currentTurnOrder],
            new() { startTurnGA });

        yield break;
    }
}