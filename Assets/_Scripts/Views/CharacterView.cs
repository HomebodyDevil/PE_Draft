using System;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private CharacterData _defaultCharacterData;
    public Character Character { get; private set; }

    private void Awake()
    {
        SetCharacter();
    }

    public void SetCharacter(Character character=null)
    {
        if (character == null && _defaultCharacterData != null)
        {
            Character = new(_defaultCharacterData);
            return;
        }
        
        Character = character;
    }
}
