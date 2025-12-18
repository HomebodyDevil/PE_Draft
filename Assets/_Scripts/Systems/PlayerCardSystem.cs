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

    public float UseCardDistance { get; private set; }

    private Coroutine _drawCardsCoroutine;

    private int _pendingDrawCount = 0;
    
    private void Start()
    {
        SetupDeck();
        SetUseCardDistance();
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

    public void SetUseCardDistance()
    {
        float screenHeight = Screen.height;
        UseCardDistance = screenHeight * ConstValue.USE_CARD_DISTANCE_RATIO;
    }

    /// <summary>
    /// 카드를 Deck에서 뽑을 때 사용하는 함수.
    /// </summary>
    /// <param name="count">뽑을 카드의 개수</param>
    public void DrawCards(int count)
    {
        _pendingDrawCount += count;
        
        if (_drawCardsCoroutine == null)
        {
            _drawCardsCoroutine = StartCoroutine(DrawCardsCoroutine());
        }
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

    public IEnumerator DrawCardsCoroutine()
    {
        int loopCnt = 0;
        while (_pendingDrawCount > 0 && loopCnt < ConstValue.MAX_LOOP)
        {
            if (Deck.Count < _pendingDrawCount)
            {
                yield return RefillDeck();
            }

            if (Deck.Count == 0)
                break;

            yield return DrawCardCoroutine();
            
            _pendingDrawCount--;
        }

        _pendingDrawCount = 0;
        _drawCardsCoroutine = null;
    }

    private IEnumerator DrawCardCoroutine()
    {
        if (Deck.Count == 0)
        {
            yield return RefillDeck();
        }
        
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

    public IEnumerator RefillDeck()
    {
        Deck.AddRange(Graveyard);
        Deck.Shuffle();
        
        Graveyard.Clear();

        yield break;
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
