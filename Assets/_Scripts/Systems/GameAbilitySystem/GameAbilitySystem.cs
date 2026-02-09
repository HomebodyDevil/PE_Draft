using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class ReactionKey : IEquatable<ReactionKey>
// {
//     // Type: 해당 Type(GameAbility)에 대한 Reaction
//     // Character: 해당 Reaction을 수행하는 Responder
//     // ReactionTarget: 해당 Reaction 수행에 대한 Target
//     // ReactionTiming: Reaction을 수행할 시점(타이밍)
//     
//     public Type AbilityForReaction { get; private set; }
//     public Character Responder { get; private set; }
//     public ReactionTarget RespondTarget { get; private set; }
//
//     public ReactionKey(
//         Type abilityForReaction,
//         Character responder,
//         ReactionTarget respondTarget)
//     {
//         AbilityForReaction = abilityForReaction;
//         Responder = responder;
//         RespondTarget = respondTarget;
//     }
//
//     public bool Equals(ReactionKey other)
//     {
//         if (other is null) return false;
//         if (ReferenceEquals(this, other)) return true;
//         return Equals(AbilityForReaction, other.AbilityForReaction) && 
//                Equals(Responder, other.Responder) && 
//                RespondTarget == other.RespondTarget;
//     }
//
//     public override bool Equals(object obj)
//     {
//         if (obj is null) return false;
//         if (ReferenceEquals(this, obj)) return true;
//         if (obj.GetType() != GetType()) return false;
//         return Equals((ReactionKey)obj);
//     }
//
//     public override int GetHashCode()
//     {
//         return HashCode.Combine(AbilityForReaction, Responder, (int)RespondTarget);
//     }
// }

class PerformGameAbilityContext
{
    public Character Caster { get; private set; }
    public GameAbility GameAbility { get; private set; }
    public List<Character> Targets { get; private set; }

    public PerformGameAbilityContext(
        Character caster, 
        GameAbility gameAbility)
    {
        Caster = caster;
        GameAbility = gameAbility;
    }
}

public class GameAbilitySystem : Singleton<GameAbilitySystem>
{
    // private static Dictionary<ReactionKey, List<GameAbility>> _preReactions = new();
    // private static Dictionary<ReactionKey, List<GameAbility>> _postReactions = new();

    // 해당 Type(GameAbility)에 대한 Reaction들에 대한 List.
    private static Dictionary<Type, List<ReactionContext>> _preReactions = new();
    private static Dictionary<Type, List<ReactionContext>> _postReactions = new();
    
    // 해당 Type(GameAbility)에 대한 Performer(IEnumerator)를 반환합니다.
    private static Dictionary<Type, Func<GameAbility, IEnumerator>> _performers = new();
    
    // 한 카드가 여러 효과를 지니고 있을 경우, 이 List에 넣어두었다가 순차적으로 수행토록 한다.
    // GA의 ReactionGA가 발동해야 한다면, 이 리스트에 추가하여 차후 수행되도록 한다.
    // 확실하진 않지만, Post Reaction들이 여기에 쌓일 것 같다.
    private Queue<PerformGameAbilityContext> _piledGameAbility = new();
    private Queue<PerformGameAbilityContext> _gameAbilityBuffer = new();
    
    public bool IsPerforming { get; private set; } = false;
    //private int _reactionCounter = ConstValue.REACTION_MAX_CHAIN;

    private Coroutine _performAbilityFlowCoroutine;

    private void OnDisable()
    {
        if (_performAbilityFlowCoroutine != null) StopCoroutine(_performAbilityFlowCoroutine);
    }

    /// <summary></summary>
    /// <param name="caster">GameAbility 시전자</param>
    // 아마 CardView에서 이 함수를 호출할 확률이 클 것인데..
    // return 값을 토대로 Perform이 수행되는지 아닌지에 대한 여부를 알 수 있도록 함.
    // success 변수는 안 쓸 것 같음.
    public void RequestPerformGameAbility(
        Character caster, 
        List<GameAbility> gameAbilities)    //, ref bool success
    {
        if (IsPerforming)
        {
            foreach (var gameAbility in gameAbilities)
            {
                PerformGameAbilityContext gaCtx = new(caster, gameAbility);
                _gameAbilityBuffer.Enqueue(gaCtx);
            }
            
            //success = false;
        }   //else success = true;
        else
        {
            foreach (var gameAbility in gameAbilities)
            {
                PerformGameAbilityContext gaCtx = new(caster, gameAbility);
                _piledGameAbility.Enqueue(gaCtx);
            }
        
            _performAbilityFlowCoroutine = StartCoroutine(PerformGameAbilitySequence());   
        }
    }

    private IEnumerator PerformGameAbilitySequence()
    {
        IsPerforming = true;

        int performCount = 0;
        while (_piledGameAbility.Count > 0 && performCount < ConstValue.MAX_PERFORM_CHAIN_COUNT)
        {
            var ctx = _piledGameAbility.Dequeue();
            yield return GameAbilityFlowCoroutine(ctx);

            performCount++;
        }
        
        performCount = 0;
        while (_gameAbilityBuffer.Count > 0 && performCount < ConstValue.MAX_PERFORM_CHAIN_COUNT)
        {
            var ctx = _gameAbilityBuffer.Dequeue();
            yield return GameAbilityFlowCoroutine(ctx);

            performCount++;
        }
        
        // foreach (var gameAbility in _piledGameAbility)
        // {
        //     yield return GameAbilityFlowCoroutine(gameAbility);
        // }

        _piledGameAbility.Clear();
        _gameAbilityBuffer.Clear();
        
        IsPerforming = false;
    }
    
    private IEnumerator GameAbilityFlowCoroutine(PerformGameAbilityContext gaCtx)
    {
        yield return PerformReaction(gaCtx, PEEnum.ReactionTiming.Pre);
        yield return PerformGameAbility(gaCtx);
        yield return PerformReaction(gaCtx, PEEnum.ReactionTiming.Post);
    }

    private IEnumerator PerformGameAbility(PerformGameAbilityContext gaCtx)
    {
        //Debug.Log("Performing Game Ability");

        if (_performers.TryGetValue(gaCtx.GameAbility.GetType(), out Func<GameAbility, IEnumerator> performer))
        {
            //Debug.Log("Found Performer");
            yield return performer(gaCtx.GameAbility);
        }
        else
        {
            Debug.Log("Cant Find Performer");
        }
    }

    private IEnumerator PerformReaction(PerformGameAbilityContext ctx, PEEnum.ReactionTiming timing)
    {
        Debug.Log($"Performing {timing.ToString()} Reaction");
        var reactions = timing ==  PEEnum.ReactionTiming.Pre ? _preReactions : _postReactions;
        Type gaType = ctx.GameAbility.GetType();

        // gaType의 Reaction들을 순회한다.
        // 즉, ReactionContext들을 순회.
        if (reactions.TryGetValue(gaType, out var reactionCtxs))
        {
            var executingReactionCtxs = reactionCtxs.ToArray();
            
            foreach (var reactionCtx in executingReactionCtxs)
            {
                if (reactionCtx.NeedResponder && !reactionCtx.IsValid())
                {
                    Debug.Log("Invalid reaction context");
                    continue;
                }

                if (reactionCtx.ReactionCount == 0)
                {
                    Type reactoinGAType = reactionCtx.ReactionGA.GetType();
                    
                    RemoveReaction(
                        reactionCtx.TriggerType,
                        reactoinGAType,
                        reactionCtx.ReactionPerformer,
                        reactionCtx.ReactionTiming);
                }
            
                List<Character> reactionTargets =
                    FindReactionTargets(ctx, reactionCtx.ReactionPerformer, reactionCtx.ReactionTarget);

                if (reactionCtx.ReactionGA is TargetGameAbility targetGameAbility)
                {
                    targetGameAbility.Targets.Clear();
                    targetGameAbility.Targets.AddRange(reactionTargets);
                }

                if (reactionCtx.ReactionCount != ConstValue.INFINITE_TURN_COUNT)
                {
                    reactionCtx.ReduceReactionCount(1);
                }
            
                PerformGameAbilityContext gaCtx = new(reactionCtx.ReactionPerformer, reactionCtx.ReactionGA);

                yield return PerformGameAbility(gaCtx);
            }
        }

        yield break;
    }

    private List<Character> FindReactionTargets(
        PerformGameAbilityContext ctx, 
        Character responder, 
        PEEnum.ReactionTarget targetType)
    {
        List<Character> targets = new();
        
        switch (targetType)
        {
            case PEEnum.ReactionTarget.All:
                targets.AddRange(PlayerSystem.Instance.PlayerCharacters);
                targets.AddRange(EnemySystem.Instance.EnemyCharacters);
                break;
            case PEEnum.ReactionTarget.Caster:
                targets.Add(ctx.Caster);
                break;
            case PEEnum.ReactionTarget.Friendly:
                // 차후 FriendlySystem을 추가할지 확실X...
                // 추가한다면, Hostile에서도 responder가 Enemy일 경우, 추가할 수 있도록 해야 할듯.
                break;
            case PEEnum.ReactionTarget.Hostile:
                List<Character> hostileList = TeamSystem.Instance.GetHostileTeamAgents(responder.TeamType.Team);
                targets.AddRange(hostileList);
                break;
            default:
                break;
        }
        
        return targets;
    }

    /// <summary>
    /// T에 대한 Reaction을 등록합니다.
    /// </summary>
    /// <param name="responder"></param>
    /// <param name="reactionGA"></param>
    /// <param name="reactionTarget"></param>
    /// <param name="reactionCount"></param>
    /// <param name="timing"></param>
    /// <param name="needResponder"></param>
    /// <typeparam name="T">Reaction을 수행하게 될 일종의 트리거</typeparam>
    public void AddReaction<T>(
        PEEnum.ReactionTiming timing,   
        Character responder, 
        GameAbility reactionGA, 
        PEEnum.ReactionTarget reactionTarget, 
        int reactionCount, 
        bool needResponder) where T : GameAbility
    {
        var list = timing == PEEnum.ReactionTiming.Pre ? _preReactions : _postReactions;

        Type triggerType = typeof(T);
        ReactionContext reactionCtx = 
            new(responder, 
                reactionGA, 
                reactionTarget, 
                reactionCount, 
                timing,
                needResponder,
                triggerType);

        if (list.ContainsKey(triggerType))
        {
            list[triggerType].Add(reactionCtx);
        }
        else
        {
            list[triggerType] = new() { reactionCtx };
        }
        
        //responder.AddAddedReaction(reactionGA, timing);
        responder.AddAddedReaction(reactionCtx);
    }

    // responder가 hold하고 있는, 그가 등록한 Reaction들의 리스트를 확인한다.(Timing이 맞는)
    // T : Reaction의 Trigger의 타입
    // T1 Type에 해당하는 Reaction들을 GameAbilitySystem 내에서 지운다. 
    // 타입은 정확히 일치해야 함.
    // public void RemoveReaction<TTrigger, TReaction>(
    //     Character responder, 
    //     PEEnum.ReactionTiming timing) 
    //     where TTrigger : GameAbility 
    //     where TReaction : GameAbility
    // {
    //     if (responder.AddedReactions.TryGetValue(timing, out var respondersList))
    //     {
    //         for (int i = respondersList.Count - 1; i >= 0; i--)
    //         {
    //             if (respondersList[i].GetType() == typeof(TReaction))
    //                 respondersList.RemoveAt(i);
    //         }
    //     }
    //     
    //     var systemReactionDict = timing == PEEnum.ReactionTiming.Pre ? _preReactions : _postReactions;
    //     Type reactionType = typeof(TReaction);
    //     
    //     if (!systemReactionDict.TryGetValue(typeof(TTrigger), out var systemReactionList))
    //     {
    //         Debug.Log("Cant find systemReactionList");
    //         return;
    //     }
    //
    //     for (int i = systemReactionList.Count - 1; i >= 0; i--)
    //     {
    //         var ctx = systemReactionList[i];
    //         if (ReferenceEquals(ctx.ReactionPerformer, responder) && ctx.ReactionGA.GetType() == reactionType)
    //         {
    //             systemReactionList.RemoveAt(i);
    //         }
    //     }
    // }
    
    public void RemoveReaction(
        Type triggerType,
        Type reactionType,
        Character responder, 
        PEEnum.ReactionTiming timing) 
    {
        if (responder.AddedReactions.TryGetValue(timing, out var respondersList))
        {
            for (int i = respondersList.Count - 1; i >= 0; i--)
            {
                if (respondersList[i].GetType() == reactionType)
                {
                    respondersList.RemoveAt(i);
                }
            }
        }
        
        var systemReactionDict = timing == PEEnum.ReactionTiming.Pre ? _preReactions : _postReactions;
        
        if (!systemReactionDict.TryGetValue(triggerType, out var systemReactionList))
        {
            Debug.Log("Cant find systemReactionList");
            return;
        }

        for (int i = systemReactionList.Count - 1; i >= 0; i--)
        {
            var ctx = systemReactionList[i];
            if (ReferenceEquals(ctx.ReactionPerformer, responder) && ctx.ReactionGA.GetType() == reactionType)
            {
                systemReactionList.RemoveAt(i);
            }
        }
    }
    
    public void RemoveReaction(ReactionContext reactionContext)
    {
        
    }
    
    public void AddPerformer<T>(Func<T, IEnumerator> performer) where T : GameAbility
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAbility ga) => performer((T)ga);
        
        _performers[type] = wrappedPerformer;
    }

    public void RemovePerformer<T>() where T : GameAbility
    {
        Type type = typeof(T);
        _performers.Remove(type);
    }
}
