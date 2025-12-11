using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ReactionTiming
{
    Pre,
    Post,
}

// Reaction을 시전할 때, 시전자가 Reaction을 수행할 대상.
// i.e. 임의의 Enemy가 Reaction을 등록한 상황.
// All : Enemy든 Player든 어떤 Action 모두가 Reaction Trigger의 대상.
// Friendly : Enemy 기준, Enemy의 Friendly(Enemy)의 Actino이 대상. 
// Hostile : Enemy 기준, Enemy의 Hostile(Player)의 Action이 대상.
public enum ReactionTarget
{
    All,
    Caster,
    Friendly,
    Hostile,
}

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

public class ReactionContext
{
    public Character ReactionPerformer { get; private set; } 
    public GameAbility ReactionGA { get; private set; }
    public ReactionTarget ReactionTarget { get; private set; }
    public int ReactionCount { get; private set; }

    public ReactionContext(
        Character reactionPerformer,
        GameAbility reactionGA,
        ReactionTarget reactionTarget,
        int reactionCount)
    {
        ReactionPerformer = reactionPerformer;
        ReactionGA = reactionGA;
        ReactionTarget = reactionTarget;
        ReactionCount = reactionCount;
    }

    public bool Check(Character reactionPerformer, GameAbility reactionGA)
    {
        if (reactionPerformer == null) return false;
        return Equals(ReactionPerformer, reactionPerformer) && 
               reactionGA?.GetType() == ReactionGA?.GetType();
    }
    
    public bool Same<T>(Character reactionPerformer, T reactionGA) where T : GameAbility
    {
        if (reactionPerformer == null || reactionGA == null)
        {
            Debug.Log("null ref");
            return false;
        }
        return Equals(ReactionPerformer, reactionPerformer) && ReactionGA?.GetType() == reactionGA.GetType();
    }
}

public class GameAbilitySystem : Singleton<GameAbilitySystem>
{
    public Action<Character> OnPerformGameAbility;
    
    private List<GameAbility> _reactions = new();
    
    // private static Dictionary<ReactionKey, List<GameAbility>> _preReactions = new();
    // private static Dictionary<ReactionKey, List<GameAbility>> _postReactions = new();

    private static Dictionary<Type, List<ReactionContext>> _preReactions = new();
    private static Dictionary<Type, List<ReactionContext>> _postReactions = new();
    
    // 해당 Type(GameAbility)에 대한 Performer(IEnumerator)를 반환합니다.
    private static Dictionary<Type, Func<GameAbility, IEnumerator>> _performers = new();
    
    // 한 카드가 여러 효과를 지니고 있을 경우, 이 List에 넣어두었다가 순차적으로 수행토록 한다.
    private List<GameAbility> _piledGameAbility = new();
    
    public bool IsPerforming { get; private set; } = false;
    //private int _reactionCounter = ConstValue.REACTION_MAX_CHAIN;

    private Coroutine _performAbilityFlowCoroutine;

    private void OnEnable()
    {
        OnPerformGameAbility += PerformGameAbility;
    }

    private void OnDisable()
    {
        OnPerformGameAbility -= PerformGameAbility;
        
        if (_performAbilityFlowCoroutine != null) StopCoroutine(_performAbilityFlowCoroutine);
    }

    /// <summary></summary>
    /// <param name="caster">GameAbility 시전자</param>
    private void PerformGameAbility(Character caster)
    {
        if (IsPerforming) return;
        _performAbilityFlowCoroutine = StartCoroutine(PerformGameAbilitySequence());
    }

    private IEnumerator PerformGameAbilitySequence()
    {
        IsPerforming = true;

        foreach (var gameAbility in _piledGameAbility)
        {
            yield return GameAbilityFlowCoroutine(gameAbility);
        }

        _piledGameAbility.Clear();
        IsPerforming = false;
    }
    
    private IEnumerator GameAbilityFlowCoroutine(GameAbility gameAbility)
    {
        yield break;
    }

 /// <summary>
 /// T에 대한 Reaction을 등록합니다.
 /// </summary>
 /// <param name="responder"></param>
 /// <param name="reactionGA"></param>
 /// <param name="reactionTarget"></param>
 /// <param name="reactionCount"></param>
 /// <param name="timing"></param>
 /// <typeparam name="T">Reaction을 수행하게 될 일종의 트리거</typeparam>
    public void AddReaction<T>(
        Character responder, 
        GameAbility reactionGA, 
        ReactionTarget reactionTarget, 
        int reactionCount, 
        ReactionTiming timing) where T : GameAbility
    {
        var list = timing == ReactionTiming.Pre ? _preReactions : _postReactions;

        Type triggerType = typeof(T);
        ReactionContext reactionCtx = new(responder, reactionGA, reactionTarget, reactionCount);

        if (list.ContainsKey(triggerType))
        {
            list[triggerType].Add(reactionCtx);
        }
        else
        {
            list[triggerType] = new() { reactionCtx };
        }
        
        responder.AddAddedReaction(reactionGA, timing);
    }

    // responder가 hold하고 있는, 그가 등록한 Reaction들의 리스트를 확인한다.(Timing이 맞는)
    // T : Reaction의 Trigger의 타입
    // T1 Type에 해당하는 Reaction들을 GameAbilitySystem 내에서 지운다. 
    // 타입은 정확히 일치해야 함.
    public void RemoveReaction<TTrigger, TReaction>(
        Character responder, 
        ReactionTiming timing) 
        where TTrigger : GameAbility 
        where TReaction : GameAbility
    {
        if (responder.AddedReactions.TryGetValue(timing, out var respondersList))
        {
            for (int i = respondersList.Count - 1; i >= 0; i--)
            {
                if (respondersList[i].GetType() == typeof(TReaction))
                    respondersList.RemoveAt(i);
            }
        }
        
        var systemReactionDict = timing == ReactionTiming.Pre ? _preReactions : _postReactions;
        Type reactionType = typeof(TReaction);
        
        if (!systemReactionDict.TryGetValue(typeof(TTrigger), out var systemReactionList))
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
