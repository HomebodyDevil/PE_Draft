using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    public InBattleCard LastUsedCard;
    
    private void OnEnable()
    {
        GameAbilitySystem.Instance.AddPerformer<PlayPlayerardGA>(PlayPlayerCardPerformer);
        GameAbilitySystem.Instance.AddPerformer<DrawCardsGA>(DrawCardsPerformer);
        GameAbilitySystem.Instance.AddPerformer<DiscardPlayerCardsGA>(DiscardPlayerCardsPerformer);
    }

    private void OnDisable()
    {
        GameAbilitySystem.Instance.RemovePerformer<PlayPlayerardGA>();
        GameAbilitySystem.Instance.RemovePerformer<DrawCardsGA>();
        GameAbilitySystem.Instance.RemovePerformer<DiscardPlayerCardsGA>();
    }

    private IEnumerator PlayPlayerCardPerformer(PlayPlayerardGA playCardGa)
    {
        Debug.Log("Play Card Performer");

        LastUsedCard = playCardGa.InBattleCardToPlay;
        InBattleCard inBattleCard = playCardGa.InBattleCardToPlay;
        
        foreach (var ga in inBattleCard.BattleCard.TargetAbility)
        {
            if (ga is TargetGameAbility targetGa)
            {
                ga.SetExecutor(PEEnum.GAExecutor.Player);
                targetGa.SetTargets(BattleSystem.Instance.CurrentTargets);
            }
        }

        BattleSystem.Instance?.OnClearTargets?.Invoke();

        List<GameAbility> gaForRequest = new();
        gaForRequest.AddRange(inBattleCard.BattleCard.TargetAbility);
        gaForRequest.AddRange(inBattleCard.BattleCard.NonTargetAbility);

        Debug.Log("caster를 null로 했는데, 나중에 고치자");
        GameAbilitySystem.Instance?.RequestPerformGameAbility(
            null,
            gaForRequest);
        //Debug.Log($"Hand Cnt: {PlayerCardSystem.Instance.Hand.Count}");

        PlayerCardSystem.Instance?.OnCardMoveToGraveyard?.Invoke(inBattleCard);

        yield break;
    }

    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGa)
    {
        PlayerCardSystem.Instance.DrawCards(drawCardsGa.DrawCount);
        yield break;
    }

    private IEnumerator DiscardPlayerCardsPerformer(DiscardPlayerCardsGA discardPlayerCardsGa)
    {
        Debug.Log("Discard Player Cards Performer");
        yield break;
    }
}
