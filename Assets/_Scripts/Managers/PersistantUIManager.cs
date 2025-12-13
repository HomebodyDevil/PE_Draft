using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ShowCardsType
{
    None,
    Deck,
    BattleDeck,
    BattleGraveyard,
}

public class PersistantUIManager : Singleton<PersistantUIManager>
{
    public Action<bool, ShowCardsType> OnShowCardsExhibition;
    
    [SerializeField] private GameObject _exhibitCardViewPrefab;
    [SerializeField] private Camera _persistantUiCamera;
    [SerializeField] private Transform _showCardsPanel;
    [SerializeField] private Transform _showCardsContainer;

    private List<GameObject> _usingExhibitCards = new();
    private List<GameObject> _storedExhibitCards = new();
    
    protected override void Awake()
    {
        base.Awake();
        
        if (_persistantUiCamera == null) transform.AssignChildVar<Camera>("PersistantUICamera", ref _persistantUiCamera);
        if (_showCardsPanel == null) transform.AssignChildVar<Transform>("ShowCardsPanel", ref _showCardsPanel);
        if (_showCardsContainer == null) transform.AssignChildVar<Transform>("ShowCardsContainer", ref _showCardsContainer);
    }

    private void OnEnable()
    {
        OnShowCardsExhibition += ShowCardsExhibition;
    }

    private void OnDisable()
    {
        OnShowCardsExhibition -= ShowCardsExhibition;
    }

    private void ShowCardsExhibition(bool show, ShowCardsType type =  ShowCardsType.None)
    {
        _showCardsPanel.gameObject.SetActive(show);
        if (type == ShowCardsType.None) return;
        
        SetShowCardsPanel(type);
    }

    private void SetShowCardsPanel(ShowCardsType type)
    {
        ClearExhibitCards();
        
        List<Card> cards = new();
        switch (type)
        {
            case ShowCardsType.Deck:
                cards = PlayerStatusService.Instance.PlayerStatus.PlayerDeck;
                break;
            case ShowCardsType.BattleDeck:
                foreach (var card in PlayerCardSystem.Instance.Deck)
                    cards.Add(card.BattleCard);
                break;
            case ShowCardsType.BattleGraveyard:
                foreach (var card in PlayerCardSystem.Instance.Graveyard)
                    cards.Add(card.BattleCard);
                break;
            case ShowCardsType.None:
                break;
            default:
                break;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            GameObject exhibitCardViewGO;
            if (_storedExhibitCards.Count > 0)
            {
                exhibitCardViewGO = _storedExhibitCards.First();
                _storedExhibitCards.Remove(exhibitCardViewGO);
            }
            else
            {
                exhibitCardViewGO = Instantiate(_exhibitCardViewPrefab, _showCardsContainer);
            }
            
            exhibitCardViewGO.SetActive(true);
            exhibitCardViewGO.GetComponent<ExhibitCardView>()?.SetCardVisual(cards[i]);
            
            _usingExhibitCards.Add(exhibitCardViewGO);
        }
    }

    private void ClearExhibitCards()
    {
        foreach (var usingCard in _usingExhibitCards)
            usingCard.SetActive(false);
        
        _storedExhibitCards.AddRange(_usingExhibitCards);
        _usingExhibitCards.Clear();
    }

    public void ShowCardsButtonClick(int type)
    {
        OnShowCardsExhibition?.Invoke(true, (ShowCardsType)type);
    }
}
