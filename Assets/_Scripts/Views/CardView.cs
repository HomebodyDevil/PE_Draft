using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public InBattleCard _card { get; private set; }

    [SerializeField] public TextMeshProUGUI cardNameTMP;
    [SerializeField] public TextMeshProUGUI cardDescriptionTMP;
    //[SerializeField] private TextMeshProUGUI costTMP;
    
    private void Awake()
    {
        transform.AssignChildVar<TextMeshProUGUI>("CardNameText", ref cardNameTMP);
        transform.AssignChildVar<TextMeshProUGUI>("CardDescriptionText", ref cardDescriptionTMP);
    }
    
    public void SetCardView(InBattleCard card)
    {
        _card = card;
        
        SetCardViewNameText(card.BattleCard.Name);
        SetCardViewDescriptionText(card.BattleCard.Description);
        SetCardViewCostText(card.BattleCard.Cost);
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
        Debug.Log($"SetCardViewCostText: {newCost}");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (_card.BattleCard.CardPlayType)
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
        switch (_card.BattleCard.CardPlayType)
        {
            case ECardPlayType.Playable:
                OnPlayablePointerUp(eventData);
                break;
            case ECardPlayType.Targetable:
                OnTargetablePointerUp(eventData);
                break;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        switch (_card.BattleCard.CardPlayType)
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
        Debug.Log("OnPlayablePointerDown");
    }

    private void OnTargetablePointerDown(PointerEventData eventData)
    {
        Debug.Log("OnTargetablePointerDown");
    }
    
    private void OnPlayablePointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPlayablePointerUp");
    }

    private void OnTargetablePointerUp(PointerEventData eventData)
    {
        Debug.Log("OnTargetablePointerUp");
    }
    
    private void OnPlayablePointerDrag(PointerEventData eventData)
    {
        Debug.Log("OnPlayablePointerDrag");
    }

    private void OnTargetablePointerDrag(PointerEventData eventData)
    {
        Debug.Log("OnTargetablePointerDrag");
    }
}
