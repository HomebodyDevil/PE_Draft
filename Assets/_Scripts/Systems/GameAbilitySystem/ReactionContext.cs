using System;
using PEEnum;
using UnityEngine;

public class ReactionContext
{
    public Character ReactionPerformer { get; private set; } 
    public GameAbility ReactionGA { get; private set; }
    public PEEnum.ReactionTarget ReactionTarget { get; private set; }   // ReactionGA의 Target으로 둘 것.
    // ReactionCount : Reaction을 수행할 수 있는 턴
    // i.e. ReactionCount가 3이다 -> 3턴 후, Reaction을 제거.
    // -1234면, 무한으로 동작하는 걸로 설정할지 고민중.
    public int ReactionCount { get; private set; }
    public bool NeedResponder { get; private set; } // Responder Character가 없어도 수행할 수 있는 Reaction인지 확인하기 위함.
    public Type TriggerType { get; private set; }   // Reaction의 Trigger가 될 GA(GA..의 Type)
    // Reaction의 Trigger GA를 수행한 자의 TeamType
    // 수행한 자가 PlayerCharacter... Enemy...일 경우, Reaction이 동작하도록 설정하기 위함.
    public Team TriggerTeamType { get; private set; }      
    public PEEnum.ReactionTiming ReactionTiming { get; private set; }

    public ReactionContext(
        Character reactionPerformer,
        GameAbility reactionGA,
        PEEnum.ReactionTarget reactionTarget,
        int reactionCount,
        PEEnum.ReactionTiming reactionTiming,
        bool needResponder = false,
        Team triggerTeamType = Team.None,
        Type triggerType = null)
    {
        ReactionPerformer = reactionPerformer;
        ReactionGA = reactionGA;
        ReactionTarget = reactionTarget;
        ReactionCount = reactionCount;
        NeedResponder = needResponder;
        TriggerType = triggerType;
        TriggerTeamType = triggerTeamType;
        ReactionTiming = reactionTiming;
    }

    public void ReduceReactionCount(int amount = 1)
    {
        ReactionCount -= amount;
    }

    public bool IsValid()
    {
        if (ReactionPerformer == null)
        {
            Debug.Log("ReactionPerformer is null");
            return false;
        }

        if (ReactionGA == null)
        {
            Debug.Log("ReactionGA is null");
            return false;
        }

        if (ReactionTarget == PEEnum.ReactionTarget.None)
        {
            Debug.Log("ReactionTarget is None");
            return false;
        }

        return true;
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
