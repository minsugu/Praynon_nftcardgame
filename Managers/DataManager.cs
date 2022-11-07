using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.IO;
using static ImportNFTIPFS;
using System.Threading;

public interface ILoader<key, value>
{
    Dictionary<key, value> MakeDict();
}

public class DataManager 
{
    List<Card> normalCardBuffer = new List<Card>();
    List<Card> nftCardBuffer = new List<Card>();
    List<Card> invenBuffer = new List<Card>();

    public int normalCnt = 0;
    public int nftCnt = 0;
    public int invenCnt = 0;

    public List<Card> GetNormalCardBuffer()
    {
        return normalCardBuffer;
    }

    public List<Card> GetNftCardBuffer()
    {
        return nftCardBuffer;
    }

    public List<Card> GetInvenBuffer()
    {
        return invenBuffer;
    }

    public bool PushCardToBuffer(Card _card)
    {
        if (_card.type == "NFT")
        {
            if (nftCnt < 3)
            {
                if (nftCardBuffer.Contains(_card))
                {
                    Debug.Log("같은 NFT카드는 하나만 보유 가능");
                    if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Inventory)
                        MessageManager.Inst.SetMessage("NFT카드는 덱에 중복으로 넣을 수 없습니다.");
                    return false;
                }

                nftCardBuffer.Add(_card);
                nftCnt++;
                invenBuffer.Add(_card);
                invenCnt++;
                Debug.Log($"@@ NFT Buffer : {nftCnt}개 @@");
                return true;
            }
            else
            {
                Debug.Log("NFT 카드는 3개까지 보유 가능");
                if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Inventory)
                    MessageManager.Inst.SetMessage("NFT카드는 최대 3장 넣을 수 있습니다.");
                return false;
            }
        }
        else
        {
            if (normalCardBuffer.Count < 20)
            {
                int _cnt = 0;
                foreach(var temp in normalCardBuffer)
                {
                    if(_card.no == temp.no)
                        _cnt++;
                }
                if (_cnt > 1)
                {
                    Debug.Log("같은 Normal 카드는 2개까지 보유 가능");
                    if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Inventory)
                        MessageManager.Inst.SetMessage("NFT카드는 2장까지만 중복으로 넣을 수 있습니다.");
                    return false;
                }

                normalCardBuffer.Add(_card);
                normalCnt++;
                invenBuffer.Add(_card);
                invenCnt++;
                Debug.Log($"@@ Normal Buffer : {normalCnt}개 @@");
                return true;
            }
            else
            {
                Debug.Log("Normal 카드는 20개까지 보유 가능");
                if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Inventory)
                    MessageManager.Inst.SetMessage("노말 카드는 최대 20장 넣을 수 있습니다.");
                return false;
            }
        }

    }
        
    public bool PopCardFromBuffer(Card _card)
    {
        if (_card.type == "NFT")
        {
            if (nftCnt > 0)
            {
                nftCardBuffer.Remove(_card);
                nftCnt--;
                invenBuffer.Remove(_card);
                invenCnt--;
                Debug.Log($"@@ NFT Buffer : {nftCnt}개 @@");
                return true;
            }
            else
            {
                Debug.Log("NFT Buffer가 비어서 POP 할 수 없습니다.");
                return false;
            }
        }
        else
        {
            if (normalCnt > 0)
            {
                normalCardBuffer.Remove(_card);
                normalCnt--;
                invenBuffer.Remove(_card);
                invenCnt--;
                Debug.Log($"@@ Normal Buffer : {normalCnt}개 @@");
                return true;
            }
            else
            {
                Debug.Log("Normal Buffer가 비어서 POP 할 수 없습니다.");
                return false;
            }
        }
    }

    public Dictionary<int, Card> CardDict { get; private set; } = new Dictionary<int, Card> ();
    public Dictionary<int, Card> NftCardDict { get; private set; } = new Dictionary<int, Card> ();

    public void Init()
    {
        // Normal
        CardDict = LoadJson<CardData, int, Card>("CardData").MakeDict();
    }
    
    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<Loader>(textAsset.text);

    }

    //NFT
    public void MakeNftDict(List<NftState> nftCardList)
    {
        foreach (NftState info in nftCardList)
        {
            Card card = new Card();
            card.no = int.Parse(info.NFTid);
            card.type = "NFT";
            card.name = info.name;
            card.explain = info.name;
            card.cost = int.Parse(info.cost);
            card.health = int.Parse(info.hp);
            card.attack = int.Parse(info.atk);
            card.sprite = Resources.Load($"Prefabs/NftCardImage/NFT{card.no}img", typeof(Sprite)) as Sprite;
            NftCardDict.Add(card.no, card);
        }
    }

}
