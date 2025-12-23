using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CardView : 
    MonoBehaviour, 
    IPointerDownHandler, 
    IPointerUpHandler, 
    IDragHandler, 
    IBeginDragHandler,
    IEndDragHandler,
    IPointerEnterHandler, 
    IPointerExitHandler
{
    private bool CanInteract = true;
    public InBattleCard InBattleCard { get; private set; }

    [SerializeField] public TextMeshProUGUI cardNameTMP;
    [SerializeField] public TextMeshProUGUI cardDescriptionTMP;
    
    [SerializeField] private Transform _visuals;
    //[SerializeField] private TextMeshProUGUI costTMP;

    private int _originalIndex = 0;
    private Vector2 _startPosition = Vector2.zero;
    private Vector2 _endPosition = Vector2.zero;
    
    private void Awake()
    {
        transform.AssignChildVar<TextMeshProUGUI>("CardNameText", ref cardNameTMP);
        transform.AssignChildVar<TextMeshProUGUI>("CardDescriptionText", ref cardDescriptionTMP);
        transform.AssignChildVar<Transform>("Visuals", ref _visuals);
    }

    private void OnEnable()
    {
        PlayerCardSystem.Instance.OnCardMoveToGraveyard += OnCardMoveToGraveyard;
    }

    private void OnDisable()
    {
        PlayerCardSystem.Instance.OnCardMoveToGraveyard -= OnCardMoveToGraveyard;
    }

    public void SetVisible(bool visible)
    {
        _visuals.gameObject.SetActive(visible);
        CanInteract = true;
    }
    
    public void SetCardView(InBattleCard card)
    {
        InBattleCard = card;
        
        SetCardViewNameText(card.BattleCard.Name);
        SetCardViewDescriptionText(card.BattleCard.Description);
        SetCardViewCostText(card.BattleCard.Cost);
    }

    public void OnCardMoveToGraveyard(InBattleCard inBattleCard)
    {
        if (inBattleCard == InBattleCard)
        {
            //Debug.Log("AAAAAA");
            InBattleCard = null;
        }
    }

    public void SetCardViewVisuals(Card card)
    {
        SetCardViewNameText(card.Name);
        SetCardViewDescriptionText(card.Description);
        SetCardViewCostText(card.Cost);
    }

    public void SetCardViewNameText(String newName)
    {
        if (cardNameTMP == null) return;
        
        cardNameTMP.text = newName;
    }

    public void SetCardViewDescriptionText(string newDescription)
    {
        if (cardDescriptionTMP == null) return;
        
        cardDescriptionTMP.text = newDescription;
    }

    public void SetCardViewCostText(int newCost)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanInteract)
        {
            return;
        }
        
        _startPosition = Mouse.current.position.ReadValue();
        _originalIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        
        if (HoveringCardViewSystem.Instance?._currentHoveringCard == InBattleCard.BattleCard)
        {
            HoveringCardViewSystem.Instance?.OnSetHoveringCardViewVisible?.Invoke(false);
        }
        
        SetVisible(true);
        
        switch (InBattleCard.BattleCard.CardPlayType)
        {
            case ECardPlayType.Playable:
                OnPlayablePointerDown(eventData);
                break;
            case ECardPlayType.Targetable:
                OnTargetablePointerDown(eventData);
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!CanInteract) return;
        
        _endPosition = Mouse.current.position.ReadValue();
        
        switch (InBattleCard.BattleCard.CardPlayType)
        {
            case ECardPlayType.Playable:
                OnPlayablePointerUp(eventData);
                break;
            case ECardPlayType.Targetable:
                OnTargetablePointerUp(eventData);
                break;
        }
        
        transform.SetSiblingIndex(_originalIndex);
        _startPosition = _endPosition = Vector2.zero;
        HoveringCardViewSystem.Instance._dragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanInteract) return;
        
        switch (InBattleCard.BattleCard.CardPlayType)
        {
            case ECardPlayType.Playable:
                OnPlayablePointerDrag(eventData);
                break;
            case ECardPlayType.Targetable:
                OnTargetablePointerDrag(eventData);
                break;
        }
    }
    
    private void OnPlayablePointerDown(PointerEventData eventData)
    {
        transform.DORotate(Quaternion.identity.eulerAngles, ConstValue.CARD_POS_RESTORE_SECONDS);
    }

    private void OnTargetablePointerDown(PointerEventData eventData)
    {
        LineSystem.Instance?.SetVisible(true);
        transform.DORotate(Quaternion.identity.eulerAngles, ConstValue.CARD_POS_RESTORE_SECONDS);
        LineSystem.Instance?.OnSetStartDrawLine?.Invoke(LineSystem.Instance.GetLinePointPosOfWorldPos(transform.position));
        LineSystem.Instance?.OnSetEndDrawLine?.Invoke(LineSystem.Instance.GetLinePointPosOfWorldPos(transform.position));
    }
    
    private void OnPlayablePointerUp(PointerEventData eventData)
    {
        // 카드를 Play할 조건(일정 거리 이상 등등)이 만족되지 않았다면
        // 본래 위치로 이동시킨다.
        // transform.DOMove(_originalPosition, ConstValue.CARD_POS_RESTORE_SECONDS);
        // transform.DORotate(_originalRotation.eulerAngles, ConstValue.CARD_POS_RESTORE_SECONDS);
        float dragDist = eventData.position.y - _startPosition.y;
        float minDist = PlayerCardSystem.Instance.UseCardDistance;
        if (dragDist > minDist)
        {
            RequestPerformGameAbility();
        }
        
        CardViewSystem.Instance?.OnLineupCardViews?.Invoke();
        transform.SetSiblingIndex(_originalIndex);
    }

    private void OnTargetablePointerUp(PointerEventData eventData)
    {
        // BattleSystem의 Target이 있다면, 해당 Target들에 대한 카드 Effect를 수행한다.
        // 이후, BattleSystem의 Target을 초기화 한다.
        BattleTargetsSystem.Instance?.OnFindTarget?.Invoke("Enemy");
        
        if (BattleTargetsSystem.Instance?.CurrentTargets.Count > 0)
        {
            RequestPerformGameAbility();
        }
        else
        {    
            transform.SetSiblingIndex(_originalIndex);
            Debug.Log("No target");
        }
        
        CardViewSystem.Instance?.OnLineupCardViews?.Invoke(); 
        LineSystem.Instance?.SetVisible(false);
        
        // if (BattleSystem.Instance?.CurrentTargets.Count > 0)
        // {
        //     foreach (var ga in InBattleCard.BattleCard.TargetAbility)
        //     {
        //         if (ga is TargetGameAbility targetGa)
        //         {
        //             ga.SetExecutor(PEEnum.GAExecutor.Player);
        //             targetGa.SetTargets(BattleSystem.Instance.CurrentTargets);
        //         }
        //     }
        //
        //     BattleSystem.Instance?.OnClearTargets?.Invoke();
        //
        //     List<GameAbility> gaForRequest = new();
        //     gaForRequest.AddRange(InBattleCard.BattleCard.TargetAbility);
        //     gaForRequest.AddRange(InBattleCard.BattleCard.NonTargetAbility);
        //
        //     Debug.Log("caster를 null로 했는데, 나중에 고치자");
        //     if (GameAbilitySystem.Instance != null &&
        //         GameAbilitySystem.Instance.RequestPerformGameAbility(null, gaForRequest))
        //     {
        //         PlayerCardSystem.Instance?.OnCardMoveToGraveyard?.Invoke(InBattleCard);
        //         //Debug.Log($"Hand Cnt: {PlayerCardSystem.Instance.Hand.Count}");
        //     }
        // }
        // else
        // {    
        //     Debug.Log("No target");
        // }

        // transform.SetSiblingIndex(_originalIndex);
        // LineSystem.Instance?.SetVisible(false);
        // CardViewSystem.Instance?.OnLineupCardViews?.Invoke(); 
    }
    
    private void OnPlayablePointerDrag(PointerEventData eventData)
    {
        transform.position = LineSystem.Instance.GetLinePointPosOfMousePos();
    }

    private void OnTargetablePointerDrag(PointerEventData eventData)
    {
        LineSystem.Instance?.OnSetEndDrawLine?.Invoke(LineSystem.Instance.GetLinePointPosOfMousePos());
        
        // 1. Raycast를 수행해서 BattleSystem의 Target을 설정할 수 있는지 확인한다.
        BattleTargetsSystem.Instance?.OnFindTarget.Invoke("Enemy");
        Debug.Log("Enemy로 설정돼있는 거 나중에 TeamSystem을 사용하는 거로 수정 예정.");
    }

    private void RequestPerformGameAbility()
    {
        PlayPlayerardGA playPlayerCardGA = new(InBattleCard);
        GameAbilitySystem.Instance.RequestPerformGameAbility(null, new() { playPlayerCardGA });
        
        CanInteract = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanInteract) return;
        if (HoveringCardViewSystem.Instance == null || HoveringCardViewSystem.Instance._dragging) return;
        
        HoveringCardViewSystem.Instance?.OnSetHoveringCardViewVisible?.Invoke(true);
        HoveringCardViewSystem.Instance?.OnSetHoveringCardViewPos?.Invoke(transform.position);
        HoveringCardViewSystem.Instance?.OnSetCurrentHoveringCard?.Invoke(InBattleCard.BattleCard);
        
        SetVisible(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CanInteract) return;
        if (HoveringCardViewSystem.Instance == null || HoveringCardViewSystem.Instance._dragging) return;
        
        if (HoveringCardViewSystem.Instance?._currentHoveringCard is { } hoveringCard &&
            hoveringCard == InBattleCard.BattleCard)
        {
            HoveringCardViewSystem.Instance?.OnSetHoveringCardViewVisible?.Invoke(false);
        }
        
        SetVisible(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanInteract) return;
        if (HoveringCardViewSystem.Instance != null) HoveringCardViewSystem.Instance._dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CanInteract) return;
        if (HoveringCardViewSystem.Instance != null) HoveringCardViewSystem.Instance._dragging = false;
    }
}
