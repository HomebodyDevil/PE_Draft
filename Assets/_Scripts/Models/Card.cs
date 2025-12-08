using UnityEngine;

public enum ECardPlayType
{
    Playable,
    Targetable,
}

// 차후, InGame내의 카드의 변경사항이 원본 카드에 영향을 끼쳐야 할 경우를 위해
// InGame에서 사용할 클래스.
// CardView는 BattleCard에 대한 것을 표시할지, 원본에 대한 것을 표시할지 결정할 수 있음.
public class InBattleCard
{
    public Card BattleCard { get; set; }
    public Card OriginalCard { get; private set; }

    public InBattleCard(
        Card battleCard,
        Card originalCard)
    {
        BattleCard = battleCard;
        OriginalCard = originalCard;
    }
}

public class Card
{
    public ECardPlayType CardPlayType { get; private set; }
    public int Cost { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public Card(CardData cardData)
    {
        CardPlayType = cardData.CardPlayType;
        Cost = cardData.CardCost;
        Name = cardData.CardName;
        Description = cardData.CardDescription;
    }
    
    public Card(
        ECardPlayType type
    )
    {
        this.CardPlayType = type;
    }

    public Card(Card other)
    {
        this.CardPlayType = other.CardPlayType;
        this.Cost = other.Cost;
        this.Name = other.Name;
        this.Description = other.Description;
    }
}
