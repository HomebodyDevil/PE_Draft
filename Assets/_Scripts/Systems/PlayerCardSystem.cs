using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardSystem : Singleton<PlayerCardSystem>
{
    public Action<InBattleCard> OnDrawCard;
    public Action<InBattleCard> OnCardMoveToGraveyard;  // 예시) CardView를 사용할 때 호출.
    public Action<InBattleCard> OnUseCard;
    
    public List<InBattleCard> Deck { get; private set; } = new();
    public List<InBattleCard> Hand { get; private set; } = new();
    public List<InBattleCard> Graveyard { get; private set; } = new();
    public List<InBattleCard> ExclusionArea { get; private set; } = new();  // 해당 게임에서 제외된 카드.

    private void Start()
    {
        SetupDeck();
    }

    private void OnEnable()
    {
        OnCardMoveToGraveyard += MoveCardToGraveyard;
    }

    private void OnDisable()
    {
        OnCardMoveToGraveyard -= MoveCardToGraveyard;
    }

    private void SetupDeck()
    {
        foreach (var card in PlayerStatusService.Instance.PlayerStatus.PlayerDeck)
        {
            InBattleCard battleCard = new(card);
            
            Deck.Add(battleCard);
        }

        Deck.Shuffle();
    }

    /// <summary>
    /// 카드를 Deck에서 뽑을 때 사용하는 함수.
    /// </summary>
    /// <param name="count">뽑을 카드의 개수</param>
    public void DrawCards(int count)
    {
        StartCoroutine(DrawCardsCoroutine(count));
    }

    /// <summary>
    /// 카드를 Graveyard로 보낼 때 사용하는 함수.
    /// </summary>
    /// <param name="card"></param>
    public void MoveCardToGraveyard(InBattleCard card)
    {
        if (card == null)
        {
            Debug.Log("Card is null");
            return;
        }
        
        StartCoroutine(MoveCardToGraveyardCoroutine(card));
    }

    public IEnumerator DrawCardsCoroutine(int count)
    {
        if (Deck.Count == 0 && Graveyard.Count == 0 && count > 0)
        {
            Debug.Log($"Cant Draw Cards. deck: {Deck.Count}, graveyard: {Graveyard.Count}");
            yield break;
        }
        
        int possibleCount = Mathf.Min(count, Deck.Count);
        int remainingCount = count - possibleCount;

        int loopCnt = 0;
        do
        {
            if (loopCnt++ > ConstValue.MAX_LOOP)
            {
                Debug.Log("Too much Loop");
                break;
            }
            
            for (int i = 0; i < possibleCount; i++)
            {
                yield return DrawCardCoroutine();
            }

            if (remainingCount > 0) RefillDeck();
            
            possibleCount = Mathf.Min(remainingCount, Deck.Count);
            remainingCount = count - possibleCount;
        } while (possibleCount > 0);
    }

    private IEnumerator DrawCardCoroutine()
    {
        Hand.Add(Deck[0]);
        OnDrawCard?.Invoke(Deck[0]);
        Deck.RemoveAt(0);

        yield break;
    }

    public IEnumerator MoveCardToGraveyardCoroutine(InBattleCard card)
    {
        if (Deck.Contains(card))
        {
            Deck.Remove(card);
        }
        else if (Hand.Contains(card))
        {
            Hand.Remove(card);
        }
        
        Graveyard.Add(card);
        
        yield break;
    }

    public void RefillDeck()
    {
        Deck.AddRange(Graveyard);
        Deck.Shuffle();
        
        Graveyard.Clear();
    }

    public void TestMoveCardToGraveyard(int index)
    {
        if (Hand.Count <= index)
        {
            Debug.Log($"Card is Null: {index}");
            return;
        }
        
        MoveCardToGraveyard(Hand[index]);
    }
}
