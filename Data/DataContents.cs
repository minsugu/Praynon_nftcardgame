using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

    #region Card
[Serializable]
public class Card
{
    public Sprite sprite;

    public int no;
    public string type;
    public string name;
    public string explain;
    public int cost;
    public int health;
    public int attack;
    
    public void SetSprite()
    {
        if(type == "NFT")
            sprite = Resources.Load($"Prefabs/NftCardImage/NFT{no}img", typeof(Sprite)) as Sprite;
        else
            sprite = Resources.Load($"Prefabs/CardImage/{no}", typeof(Sprite)) as Sprite;
    }
}

[Serializable]
public class CardData : ILoader<int, Card>
{
    public List<Card> cards = new List<Card>();

    public Dictionary<int, Card> MakeDict()
    {
        Dictionary<int, Card> dict = new Dictionary<int, Card>();
        foreach (Card card in cards)
        {
            card.sprite = Resources.Load($"Prefabs/CardImage/{card.no}", typeof(Sprite)) as Sprite;
            dict.Add(card.no, card);
        }
        return dict;
    }
       
}

#endregion