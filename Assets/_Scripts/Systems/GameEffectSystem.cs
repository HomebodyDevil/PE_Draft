using System;
using System.Collections;
using UnityEngine;

public class GameEffectSystem : Singleton<GameEffectSystem>
{
    private void Start()
    {
        GameAbilitySystem.Instance.AddPerformer<PlayerGainBlockGA>(PlayerGainBlockPerformer);
        GameAbilitySystem.Instance.AddPerformer<PlayerReduceBlockGA>(PlayerReduceBlockPerformer);

        GameAbilitySystem.Instance.AddPerformer<EnemyGainBlockGA>(EnemyGainBlockPerformer);
    }

    private void OnDisable()
    {
        GameAbilitySystem.Instance.RemovePerformer<PlayerGainBlockGA>();
        GameAbilitySystem.Instance.RemovePerformer<PlayerReduceBlockGA>();
        
        GameAbilitySystem.Instance.RemovePerformer<EnemyGainBlockGA>();
    }

    public IEnumerator PlayerGainBlockPerformer(PlayerGainBlockGA gainBlockGA)
    {
        PlayerGainBlock(gainBlockGA.TargetCharacter, gainBlockGA.BlockAmount);
        yield break;
    }
    
    public IEnumerator PlayerReduceBlockPerformer(PlayerReduceBlockGA reduceBlockGA)
    {
        PlayerReduceBlock(reduceBlockGA.TargetCharacterName, reduceBlockGA.ReduceAmount, reduceBlockGA.ReduceAll);
        yield break;
    }

    public IEnumerator EnemyGainBlockPerformer(EnemyGainBlockGA gainBlockGA)
    {
        EnemyCharacter targetEnemy = gainBlockGA.TargetEnemy;
        int blockAmount = gainBlockGA.BlockAmount;
        
        Character target = EnemySystem.Instance.FindEnemyCharacter(targetEnemy) as Character;
        if (target == null)
        {
            Debug.LogError("No target found");
            yield break;
        }
        
        GainBlock(target, blockAmount);
    }

    public IEnumerator EnemyReduceBlockPerformer(EnemyReduceBlockGA reduceBlockGA)
    {
        EnemyCharacter targetEnemy = reduceBlockGA.TargetEnemy;
        int reduceAmount = reduceBlockGA.ReduceBlockAmount;

        if (targetEnemy == null)
        {
            Debug.LogError("No target found");
            yield break;
        }

        Character target = targetEnemy as Character;
        ReduceBlock(target, reduceAmount);
    }

    public void PlayerGainBlock(PEEnum.PlayerCharacter playerCharacter, int blockAmount)
    {
        Character character = PlayerSystem.Instance.FindPlayerCharacter(playerCharacter);
        if (character == null)
        {
            Debug.Log("No player character found");
            return;
        }
        
        GainBlock(character, blockAmount);
    }

    public void GainBlock(Character character, int blockAmount)
    {
        character.Block = character.Block + blockAmount;
        PEEvent.OnCharacterGainedBlock?.Invoke(character, character.Block);
        PEEvent.OnPlayTriggerAnimation?.Invoke(character, "Block");
        //Debug.Log($"{character.CharacterName} gain block : {blockAmount}, current block : {character.Block}");
    }
    
    public void PlayerReduceBlock(string playerCharacterName, int reduceAmount, bool reduceAll = false)
    {
        Character character = PlayerSystem.Instance.FindPlayerCharacter(playerCharacterName);
        if (character == null)
        {
            Debug.LogError("No player character found");
            return;
        }

        if (reduceAll) reduceAmount = character.Block;
        reduceAmount = Math.Max(reduceAmount, 0);

        ReduceBlock(character, reduceAmount);
    }

    public void ReduceBlock(Character character, int reduceAmount)
    {
        character.Block = character.Block - reduceAmount;
        PEEvent.OnCharacterLostBlock?.Invoke(character, character.Block);
    }
}
