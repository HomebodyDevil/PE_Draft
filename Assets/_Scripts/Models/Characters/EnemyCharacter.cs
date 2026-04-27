using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyCharacter : Character, IEnemyTurnStart    
{
    private Coroutine _enemyTurnCoroutine;
    private int _turnCount = 0;

    private EnemyBehaviorPatternData _enemyBehaviorPatternData;
    
    public EnemyCharacter(CharacterData data) : base(data)
    {
        if (data is EnemyCharacterData enemyData)
            InitializeEnemyData(enemyData);
    }

    private void InitializeEnemyData(EnemyCharacterData data)
    {
        if (data.EnemyBehaviorPatternData == null)
        {
            Debug.LogWarning($"{data.CharacterName} has no EnemyBehaviorPatternData");
            return;
        }

        _enemyBehaviorPatternData = Object.Instantiate(data.EnemyBehaviorPatternData);
    }

    public void AddEnemyReactions()
    {
        // InBattle에서 Reaction을 추가한다면 이것을 사용할 예정.
        Debug.Log("AddEnemyReactions : InBattle에서 Reaction을 추가");
    }
    
    public override void StartTurn()
    {
        Debug.Log($"{CharacterName} : Enemy Turn Start");
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
        // Debug.Log("Test로 말풍선 보여주기중. 차후 수정.");
        //
        // string dialogue = $"My Turn : {_turnCount}";
        // PEEvent.OnSetDialogueBubble?.Invoke(true, this, dialogue);
        //
        // yield return new WaitForSecondsRealtime(1f);
        //
        // PEEvent.OnSetDialogueBubble?.Invoke(false, this, dialogue);

        if (_enemyBehaviorPatternData == null)
        {
            Debug.LogWarning($"_enemyBehaviorPatternData is null");
        }
        else
        {
            var turnSkillData = _enemyBehaviorPatternData.GetSkillDataForTurn(_turnCount);
            var abilityList = turnSkillData.SkillAbilities.ToList(); 
            
            foreach (var ability in abilityList)
                ability.SetCaster(this);
            
            if (turnSkillData != null)
            {
                yield return GameAbilitySystem.Instance.RequestPerformGameAbilityAndWait(
                    this,
                    abilityList);
            }
            else
                Debug.LogWarning($"turnAbility is null : turn = {_turnCount}");
        }
        
        EndCharacterTurnGA endTurnGA = new(this);
        GameAbilitySystem.Instance.RequestPerformGameAbility(this, new() { endTurnGA });

        _turnCount++;
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

    private void DoBehavior(int turnIndex)
    {
        
        if (_enemyBehaviorPatternData == null)
        {
            Debug.LogError("EnemyBehaviorPatternData is null");
            return;
        }
        
        var turnSkillData = _enemyBehaviorPatternData.GetSkillDataForTurn(turnIndex);

        if (turnSkillData == null)
        {
            Debug.LogWarning($"No SkillData found for turn {turnIndex}");
            return;
        }
        
        GameAbilitySystem.Instance.RequestPerformGameAbility(this, turnSkillData.SkillAbilities.ToList());
    }

    public void TurnStart()
    {
        //Debug.Log($"{CharacterName} : Enemy Turn Start");
    }
}
