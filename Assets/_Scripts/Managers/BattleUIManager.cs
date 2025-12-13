using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : Singleton<BattleUIManager>
{
    [SerializeField] private Canvas _buttonsUiCanvas;
    [SerializeField] private Button _deckButton;
    [SerializeField] private Button _graveyardButton;
    [SerializeField] private RectTransform _showCardsPanel;
    [SerializeField] private Transform _showCardsContainer;
    [SerializeField] private GameObject _exhibitCardViewPrefab;

    private List<Character> _playerCharacters = new();
    private List<Character> _enemyCharacters = new();
    
    private List<GameObject> _usingExhibitCards = new();
    private List<GameObject> _storedExhibitCards = new();
    
    protected override void Awake()
    {
        base.Awake();

        VarSetup();
        Setup();
    }

    private void VarSetup()
    {
        if (_buttonsUiCanvas == null) transform.AssignChildVar<Canvas>("ButtonsUICanvas", ref _buttonsUiCanvas);
        if (_deckButton == null) transform.AssignChildVar<Button>("DeckButton", ref _deckButton);
        if (_graveyardButton == null) transform.AssignChildVar<Button>("GraveyardButton", ref _graveyardButton);
        if (_showCardsPanel == null) transform.AssignChildVar<RectTransform>("ShowCardsPanel", ref _showCardsPanel);
        if (_showCardsContainer == null) transform.AssignChildVar<Transform>("ShowCardsContainer", ref _showCardsContainer);
    }

    private void Setup()
    {
        if (_buttonsUiCanvas != null)
        {
            _buttonsUiCanvas.sortingOrder = ConstValue.UI_ORDER;
        }
    }
    
    // private void ShowCardsExhibition(bool show, ShowCardsType type =  ShowCardsType.None)
    // {
    //     _showCardsPanel.gameObject.SetActive(show);
    //     if (type == ShowCardsType.None) return;
    //     
    //     SetShowCardsPanel(type);
    // }

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

    public void ShowCards(int type)
    {
        _showCardsPanel.gameObject.SetActive(true);
        if ((ShowCardsType)type == ShowCardsType.None) return;
        
        SetShowCardsPanel((ShowCardsType)type);
    }
}
