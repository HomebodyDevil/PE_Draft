using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerCharacterViewSystem : CharacterViewSystem
{
    [SerializeField] private List<Transform> _playerCharacterPositions = new(); 
    
    protected override void Awake()
    {
        base.Awake();
        SetVars();
    }

    private void Start()
    {
        
    }

    private void SetVars()
    {
        Transform tr = null;
        if (transform.AssignChildVar<Transform>("PlayerCharacterPositions", ref tr))
        {
            _playerCharacterPositions.Clear();
            tr.GetComponentsInChildren<Transform>(true, _playerCharacterPositions);
            _playerCharacterPositions.Remove(tr);
        }

        SetCharacterPositions();
    }

    private void SetCharacterPositions()
    {
        _playerCharacterPositions.Clear();

        string playerMatch = @"^PlayerCharacterPosition(100|[1-9]?\d)$";
        
        Transform[] children = transform.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == transform) continue;

            if (Regex.IsMatch(children[i].name, playerMatch))
            {
                _playerCharacterPositions.Add(children[i]);
            }
        }
    }
}
