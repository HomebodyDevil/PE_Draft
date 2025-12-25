using System;
using System.Collections;
using UnityEngine;

public class BattleEventSystem : Singleton<BattleEventSystem>
{
    public Action<Character> OnCharacterDeath;
    
    private void OnEnable()
    {
        GameAbilitySystem.Instance?.AddPerformer<CharacterDeathGA>(CharacterDeathPerformer);
    }

    private void OnDisable()
    {
        GameAbilitySystem.Instance?.RemovePerformer<CharacterDeathGA>();
    }

    public IEnumerator CharacterDeathPerformer(CharacterDeathGA characterDeathGA)
    {
        OnCharacterDeath?.Invoke(characterDeathGA.DeadCharacter);
        yield break;
    }
}
