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
                    Debug.Log("���� NFTī��� �ϳ��� ���� ����");
                    if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Inventory)
                        MessageManager.Inst.SetMessage("NFTī��� ���� �ߺ����� ���� �� �����ϴ�.");
                    return false;
                }

                nftCardBuffer.Add(_card);
                nftCnt++;
                invenBuffer.Add(_card);
                invenCnt++;
                Debug.Log($"@@ NFT Buffer : {nftCnt}�� @@");
                return true;
            }
            else
            {
                Debug.Log("NFT ī��� 3������ ���� ����");
                if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Inventory)
                    MessageManager.Inst.SetMessage("NFTī��� �ִ� 3�� ���� �� �ֽ��ϴ�.");
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
                    Debug.Log("���� Normal ī��� 2������ ���� ����");
                    if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Inventory)
                        MessageManager.Inst.SetMessage("NFTī��� 2������� �ߺ����� ���� �� �ֽ��ϴ�.");
                    return false;
                }

                normalCardBuffer.Add(_card);
                normalCnt++;
                invenBuffer.Add(_card);
                invenCnt++;
                Debug.Log($"@@ Normal Buffer : {normalCnt}�� @@");
                return true;
            }
            else
            {
                Debug.Log("Normal ī��� 20������ ���� ����");
                if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Inventory)
                    MessageManager.Inst.SetMessage("�븻 ī��� �ִ� 20�� ���� �� �ֽ��ϴ�.");
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
                Debug.Log($"@@ NFT Buffer : {nftCnt}�� @@");
                return true;
            }
            else
            {
                Debug.Log("NFT Buffer�� �� POP �� �� �����ϴ�.");
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
                Debug.Log($"@@ Normal Buffer : {normalCnt}�� @@");
                return true;
            }
            else
            {
                Debug.Log("Normal Buffer�� �� POP �� �� �����ϴ�.");
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
