using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerCharacterViewSystem : CharacterViewSystem<PlayerCharacterViewSystem>
{
    private Coroutine _setCharacterViewsBasedOnSystemCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
        SetVars();
    }

    private void Start()
    {
        MakeCharacterViews(Team.PlayerCharacter);
    }

    private void SetVars()
    {
        Transform tr = null;
        if (transform.AssignChildVar<Transform>("PlayerCharacterPositions", ref tr))
        {
            _characterPositions.Clear();
            tr.GetComponentsInChildren<Transform>(true, _characterPositions);
            _characterPositions.Remove(tr);
        }

        SetCharacterPositions();
    }

    protected override void SetCharacterPositions()
    {
        _characterPositions.Clear();

        string playerMatch = @"^PlayerCharacterPosition(100|[1-9]?\d)$";
        
        Transform[] children = transform.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == transform) continue;

            if (Regex.IsMatch(children[i].name, playerMatch))
            {
                _characterPositions.Add(children[i]);
            }
        }
    }

    protected override void MakeCharacterViews(Team team)
    {
        if (CharacterViews.Count >= ConstValue.NUMBER_OF_PLAYER_CHARACTERS)
        {
            Debug.Log("Already Made Character Views");
            return;
        }
        
        base.MakeCharacterViews(team);
    }

    public override void SetCharacterViewsBasedOnSystem(Team team)
    {
        base.SetCharacterViewsBasedOnSystem(team);
        
        if (_setCharacterViewsBasedOnSystemCoroutine != null)
        {
            Debug.Log("Setting player character views based on player system is running");
            StopCoroutine(_setCharacterViewsBasedOnSystemCoroutine);
            _setCharacterViewsBasedOnSystemCoroutine = null;
        }

        _setCharacterViewsBasedOnSystemCoroutine =
            StartCoroutine(SetCharacterViewsBasedOnSystemCoroutine());
    }

    protected override IEnumerator SetCharacterViewsBasedOnSystemCoroutine()
    {
        int loopCnt = 0;
        //Debug.Log("CharacterView들 만들어지기를 기다리기");
        while (_makeCharacterViewsCoroutine != null)
        {
            if (loopCnt++ >= ConstValue.MAX_LOOP)
            {
                Debug.Log("Max Loop in SetCharacterViewsBasedOnSystemCoroutine");
                yield break;
            }
            
            yield return new WaitForSeconds(0.02f);
        }
        
        var playerCharacters = PlayerSystem.Instance.PlayerCharacters;
        if (playerCharacters == null || playerCharacters.Count == 0)
        {
            Debug.Log("players is null or players count is 0");
            yield break;
        }
        
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            Character curr = playerCharacters[i];
            Debug.Log($"name : {curr.CharacterName}\nTeam : {curr.TeamType.Team.ToString()}");
            
            CharacterViews[i].SetCharacterView(playerCharacters[i]);
        }

        _setCharacterViewsBasedOnSystemCoroutine = null;
    }
}
