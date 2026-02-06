using System.Collections;
using UnityEngine;

public class EnemyCharacter : Character, IEnemyTurnStart    
{
    private Coroutine _enemyTurnCoroutine;
    
    public EnemyCharacter(CharacterData data) : base(data)
    {
        
    }

    public void AddEnemyReactions()
    {
        // InBattle에서 Reaction을 추가한다면 이것을 사용할 예정.
        Debug.Log("AddEnemyReactions : InBattle에서 Reaction을 추가");
    }
    
    public override void StartTurn()
    {
        Debug.Log("EnemyStartTurn");
        if (_enemyTurnCoroutine != null)
        {
            CoroutineRunnerService.Instance.StopCoroutine(_enemyTurnCoroutine);
            _enemyTurnCoroutine = null;
        }
        
        _enemyTurnCoroutine = CoroutineRunnerService.Instance.StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        // CharacterView에서도 없애줘야 함.
        Debug.Log("EnemyTurnCoroutine 타이밍");
        
        EndCharacterTurnGA endTurnGA = new(this);
        GameAbilitySystem.Instance.RequestPerformGameAbility(this, new() { endTurnGA });

        _enemyTurnCoroutine = null;
        
        yield break;

        // Transform testText = null;
        // foreach (var charView in EnemyCharacterViewSystem.Instance.EnemyCharacterViews)
        // {
        //     if (charView.Character == this)
        //     {
        //         testText = charView.Text;
        //         testText.gameObject.SetActive(true);
        //     }
        // }
        //
        // yield return new WaitForSeconds(2.0f);
        //
        // Debug.Log("Done");
        //
        // EndCharacterTurnGA endTurnGA = new(this);
        // GameAbilitySystem.Instance.RequestPerformGameAbility(this, new() { endTurnGA });
        //
        // _enemyTurnCoroutine = null;
        // testText.gameObject.SetActive(false);

        // Reaction 잘 없어지는지 확인.
        // GameAbilitySystem.Instance.RemoveReaction(
        //     Reactions[0].TriggerType,
        //     Reactions[0].ReactionGA.GetType(),
        //     this,
        //     PEEnum.ReactionTiming.Pre);
    }

    public void TurnStart()
    {
        Debug.Log($"{CharacterName} : Enemy Turn Start");
    }
}
