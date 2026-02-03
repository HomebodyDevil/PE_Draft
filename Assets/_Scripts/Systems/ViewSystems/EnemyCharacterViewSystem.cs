using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyCharacterViewSystem : CharacterViewSystem<EnemyCharacterViewSystem>
{
    //[SerializeField] private List<Transform> _enemyCharacterPositions = new();
    //public List<CharacterView> EnemyCharacterViews { get; private set; }= new();

    //private Coroutine _makeEnemyCharacterViewsCoroutine;
    private Coroutine _initialSettingCoroutine;
    private Coroutine _setCharacterViewsBasedOnSystemCoroutine;
    private int _numberOfPositions = 6;
    
    protected override void Awake()
    {
        base.Awake();
        SetVars();
    }

    private void Start()
    {
        MakeCharacterViews(Team.Enemy);
    }

    private void OnEnable()
    {
        BattleEventSystem.Instance.OnCharacterDeath += DisableEnemyCharacterView;
    }

    private void OnDisable()
    {
        BattleEventSystem.Instance.OnCharacterDeath -= DisableEnemyCharacterView;
    }

    private void OnDestroy()
    {
        if (_initialSettingCoroutine != null)
        {
            StopCoroutine(_initialSettingCoroutine);
            _initialSettingCoroutine = null;
        }
    }

    private void InitialSetting()
    {
        if (CharacterViews.Count > 0)
        {
            foreach (var enemyCharacterView in CharacterViews)
                EnemySystem.Instance.RemoveEnemyInList(enemyCharacterView.Character);
        }
    }

    private void DisableEnemyCharacterView(Character character)
    {
        SetEnableCharacterView(character, false);
    }
    
    protected override void MakeCharacterViews(Team team)
    {
        if (CharacterViews.Count >= ConstValue.NUMBER_OF_ENEMY_CHARACTERS)
        {
            Debug.Log("Already Made Character Views");
            return;
        }
        
        base.MakeCharacterViews(team);
    }
    
    protected override void SetCharacterPositions()
    {
        _characterPositions.Clear();
        
        string enemyMatch = @"^EnemyCharacterPosition(100|[1-9]?\d)$";
        
        Transform[] children = transform.GetComponentsInChildren<Transform>(true);
    
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == transform) continue;
    
            if (Regex.IsMatch(children[i].name, enemyMatch))
            {
                _characterPositions.Add(children[i]);
            }
        }
    }
    
    private void SetVars()
    {
        Transform tr = null;
        if (transform.AssignChildVar<Transform>("EnemyCharacterPositions", ref tr))
        {
            _characterPositions.Clear();
            tr.GetComponentsInChildren<Transform>(true, _characterPositions);
            _characterPositions.Remove(tr);
        }
    
        _numberOfPositions = ConstValue.NUMBER_OF_ENEMY_CHARACTERS;
    
        SetCharacterPositions();
    }
    
    public CharacterView GetCharacterView(Character character)
    {
        foreach (var characterView in CharacterViews)
        {
            if (characterView.Character == character) return characterView;
        }
    
        return null;
    }
    
    public override void SetCharacterViewsBasedOnSystem(Team team)
    {
        base.SetCharacterViewsBasedOnSystem(team);
        
        if (_setCharacterViewsBasedOnSystemCoroutine != null)
        {
            Debug.Log("_setEnemyCharacterViewBasedOnEnemySystemCoroutine is not null");
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
        
        var enemies = EnemySystem.Instance.EnemyCharacters;
        if (enemies == null || enemies.Count == 0)
        {
            Debug.Log("enemies is null or enemies count is 0");
            yield break;
        }
        
        for (int i = 0; i < enemies.Count; i++)
        {
            CharacterViews[i].SetCharacterView(enemies[i]);
        }
        
        _setCharacterViewsBasedOnSystemCoroutine = null;
    }
}
