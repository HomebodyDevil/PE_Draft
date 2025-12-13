using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Splines;

public class CardViewSystem : Singleton<CardViewSystem>
{
    public Action OnCardViewCreated;
    public Action OnLineupCardViews;
    
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Camera cardViewCam;
    [SerializeField] private Transform cardViews;
    [SerializeField] private Transform extraCardViews;
    [SerializeField] private GameObject cardViewPrefab;
    [SerializeField] private RectTransform deckButtonRT;
    [SerializeField] private RectTransform graveyardButtonRT;
    [SerializeField] private Canvas cardViewCanvas;

    [SerializeField] private List<CardView> _usingCardViewList = new();
    [SerializeField] private List<CardView> _extraCardViewList = new();

    private Coroutine _lineupCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
        VarSetup();
        Setup();
    }

    private void OnEnable()
    {
        PlayerCardSystem.Instance.OnDrawCard += DrawCardView;
        PlayerCardSystem.Instance.OnCardMoveToGraveyard += MoveCardViewToGraveyard;
        OnLineupCardViews += LineUpHandCardViews;
    }

    private void OnDisable()
    {
        PlayerCardSystem.Instance.OnDrawCard -= DrawCardView;
        PlayerCardSystem.Instance.OnCardMoveToGraveyard -= MoveCardViewToGraveyard;
        OnLineupCardViews -= LineUpHandCardViews;
    }

    private void Start()
    {
        
    }

    private void VarSetup()
    {
        if (splineContainer == null) transform.AssignChildVar<SplineContainer>("CardViewCurve", ref splineContainer);
        if (cardViewCam == null) transform.AssignChildVar<Camera>("CardViewCamera", ref cardViewCam);
        if (cardViews == null) transform.AssignChildVar<Transform>("CardViews", ref cardViews);
        if (extraCardViews == null) transform.AssignChildVar<Transform>("ExtraCardViews", ref extraCardViews);
        if (deckButtonRT == null) transform.AssignChildVar<RectTransform>("DeckButtonRT", ref deckButtonRT);
        if (graveyardButtonRT == null)
            transform.AssignChildVar<RectTransform>("GraveyardButtonRT", ref graveyardButtonRT);
        if (cardViewCanvas == null) transform.AssignChildVar<Canvas>("CardViewCanvas", ref cardViewCanvas);
    }

    private void Setup()
    {
        if (cardViewCanvas != null)
        {
            cardViewCanvas.sortingOrder = ConstValue.CARDVIEW_ORDER;
        }
    }

    /// <summary></summary>
    /// <param name="localPosition">PosX=0, PosY=0에 대한 localPosition.</param>
    private CardView CreateCardView(Vector3 localPosition)
    {
        GameObject cardView = Instantiate(cardViewPrefab, cardViews, false);
        cardView?.GetComponent<RectTransform>().SetLocalPositionAndRotation(localPosition, Quaternion.identity);

        if (cardView.TryGetComponent(out CardView cardViewComp))
        {
            cardView.GetComponent<RectTransform>().localScale = Vector3.zero;
            _usingCardViewList.Add(cardViewComp);

            return cardViewComp;
        }
        else
        {
            Debug.Log("Failed to create card view");
            return null;
        }
    }

    // PlayerCardSystem.Instance.OnDrawCard를 구독한다.
    private void DrawCardView(InBattleCard inBattleCard)
    {
        CardView cardView = GetAvailableCardView(true);
        cardView?.SetCardView(inBattleCard);
        cardView?.transform.SetParent(cardViews);

        LineUpHandCardViews();
    }

    // CARD_LINEUP_SECONDS가 CARD_DRAW_SECONDS보다 길면
    // 문제가 발생한다. cardViewRT 각각의 Tween을 캐싱하여 따로 관리하면 될 것 같긴 한데.
    // 그냥 CARD_LINEUP_SECONDS를 CARD_DRAW_SECONDS보다 짧게 했다.
    private void MoveCardViewToGraveyard(InBattleCard inBattleCard)
    {
        foreach (CardView cardView in _usingCardViewList)
        {
            if (cardView._card == null)
            {
                Debug.Log("cardView is null");
                break;
            }
            
            if (cardView._card.Equals(inBattleCard))
            {
                RectTransform cardViewRT = null;
                if (cardView.TryGetComponent<RectTransform>(out cardViewRT))
                {
                    _usingCardViewList.Remove(cardView);
                    _extraCardViewList.Add(cardView);
                    
                    cardViewRT.DOScale(Vector3.zero, ConstValue.CARD_DRAW_SECONDS);
                    cardViewRT.DOMove(graveyardButtonRT.position, ConstValue.CARD_DRAW_SECONDS);
                    cardViewRT.DORotate(Quaternion.identity.eulerAngles, ConstValue.CARD_DRAW_SECONDS);
                    
                    StartCoroutine(SetCardViewActiveCoroutine(cardViewRT.gameObject, false, ConstValue.CARD_DRAW_SECONDS));
                    
                    LineUpHandCardViews();
                }

                break;
            }
        }
    }
    
    // 일종의 pool 패턴.
    private CardView GetAvailableCardView(bool setActive = true)
    {
        CardView ret = null;
        
        if (_extraCardViewList.Count > 0)
        {
            ret = _extraCardViewList[0];
            
            _usingCardViewList.Add(_extraCardViewList[0]);
            _extraCardViewList.RemoveAt(0);
            ret.gameObject.SetActive(setActive);
        }
        else
        {
            ret = CreateCardView(deckButtonRT.localPosition);
        }

        return ret;
    }

    public void LineUpHandCardViews()
    {
        if (_lineupCoroutine != null) StopCoroutine(_lineupCoroutine);
        _lineupCoroutine = StartCoroutine(LineUpHandCardViewsCoroutine());
    }

    private IEnumerator SetCardViewActiveCoroutine(GameObject cardViewGO, bool active, float time)
    {
        yield return new WaitForSeconds(time);

        if (cardViewGO.TryGetComponent<RectTransform>(out var cardViewRT))
        {
            cardViewRT.SetParent(extraCardViews);
            cardViewRT.position = deckButtonRT.position;
        }
        
        cardViewGO.SetActive(active);
    }

    private IEnumerator LineUpHandCardViewsCoroutine()
    {
        int cardNum = _usingCardViewList.Count;

        float space = cardNum > ConstValue.MAX_HAND_CARDS
            ? (1f / (cardNum - 1))
            : (1f / (ConstValue.MAX_HAND_CARDS - 1));

        float firstCardPos = 0.5f - (cardNum - 1) * space * 0.5f;
        
        for (int i = 0; i < cardNum; i++)
        {
            RectTransform cardViewRT = _usingCardViewList[i].GetComponent<RectTransform>();
            if (cardViewRT == null)
            {
                Debug.Log($"CardView[{i}] is null");
                continue;
            }
            
            float pos = firstCardPos + space * i;
            float rot = -(2 * ConstValue.CARDVIEW_ROTATE_MAX * pos - ConstValue.CARDVIEW_ROTATE_MAX);
                
            Vector3 worldPos = splineContainer.EvaluatePosition(pos);
            
            cardViewRT.DOScale(Vector3.one, ConstValue.CARD_LINEUP_SECONDS);
            cardViewRT.DORotate(new(0, 0, rot), ConstValue.CARD_LINEUP_SECONDS);
            cardViewRT.DOMove(worldPos, ConstValue.CARD_LINEUP_SECONDS);
        }

        yield break;
    }

    // 쓸 일 없을듯?
    // public void CreateCardViewAddToHand()
    // {
    //     CreateCardView(deckButtonRT.localPosition);
    //     LineUpHandCardViews();
    // }
    
    // LineUpHandCardViews 쓰면 돼서 쓸모 없을듯.
    // private void MoveCardToHand(RectTransform cardViewRT)
    // {
    //     cardViewRT.DOLocalMove(graveyardButtonRT.localPosition, 0.5f);
    // }
}