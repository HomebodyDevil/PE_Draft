using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExhibitCardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI cardDescription;
    //[SerializeField] private TextMeshProUGUI cardCost;

    private void Awake()
    {
        if (cardImage == null) transform.AssignChildVar<Image>("CardImage", ref cardImage);
        if (cardName == null) transform.AssignChildVar<TextMeshProUGUI>("CardNameText", ref cardName);
        if (cardDescription == null)  transform.AssignChildVar<TextMeshProUGUI>("CardDescriptionText", ref cardDescription);
    }

    public void SetCardVisual(Card card)
    {
        cardName.text = card.Name;
        cardDescription.text = card.Description;
    }
}
