using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using Photon.Pun;

public class CardPrefab : MonoBehaviourPunCallbacks
{
    private SpriteRenderer[] tempSR;
    [SerializeField] SpriteRenderer cardFrame;
    [SerializeField] SpriteRenderer character;
    
    private TMP_Text[] temp;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] TMP_Text costTMP;

    [SerializeField] Sprite cardBack;

    public Card card;
    public bool isFront;
    public PRS originPRS;
    
    public PhotonView PV;


    #region components from gameManager
    [SerializeField] public Transform cardSpawnPoint;
    [SerializeField] public Transform myCardLeft;
    [SerializeField] public Transform myCardRight;
    [SerializeField] public Transform NFTMyCardLeft;
    [SerializeField] public Transform NFTMyCardRight;
    [SerializeField] public Transform MyShowCardArea;

    [SerializeField] public Transform otherCardSpawnPoint;
    [SerializeField] public Transform otherCardLeft;
    [SerializeField] public Transform otherCardRight;
    [SerializeField] public Transform NFTOtherCardLeft;
    [SerializeField] public Transform NFTOtherCardRight;
    [SerializeField] public Transform OtherShowCardArea;
    #endregion


    public void Awake()
    {
        tempSR = gameObject.GetComponentsInChildren<SpriteRenderer>(); //2개
        cardFrame = tempSR[0]; // 카드 프레임
        character = tempSR[1]; // 카드 사진 

        temp = gameObject.GetComponentsInChildren<TMP_Text>(); //4개
        nameTMP = temp[0];
        attackTMP = temp[1];
        healthTMP = temp[2];
        costTMP = temp[3];

        cardFrame.sprite = Resources.Load("Prefabs/CardGameProject_jpg/CardFrame", typeof(Sprite)) as Sprite;
        cardBack = Resources.Load("Prefabs/CardGameProject_jpg/BackCard", typeof(Sprite)) as Sprite;

        
        cardSpawnPoint = _GameManager.Inst.cardSpawnPoint;
        myCardLeft = _GameManager.Inst.myCardLeft;
        myCardRight = _GameManager.Inst.myCardRight;
        NFTMyCardLeft = _GameManager.Inst.NFTMyCardLeft;
        NFTMyCardRight = _GameManager.Inst.NFTMyCardRight;
        MyShowCardArea = _GameManager.Inst.MyShowCardArea;

        otherCardSpawnPoint = _GameManager.Inst.otherCardSpawnPoint;
        otherCardLeft = _GameManager.Inst.otherCardLeft;   
        otherCardRight = _GameManager.Inst.otherCardRight;
        NFTOtherCardLeft = _GameManager.Inst.NFTOtherCardLeft;
        NFTOtherCardRight = _GameManager.Inst.NFTOtherCardRight;
        OtherShowCardArea = _GameManager.Inst.OtherShowCardArea;

        PV = gameObject.GetComponent<PhotonView>();

        object[] data = PV.InstantiationData;
        Card card = new Card();
        card.no = (int)data.GetValue(0);
        card.type = (string)data.GetValue(1);
        card.name = (string)data.GetValue(2);
        card.explain = (string)data.GetValue(3);
        card.cost = (int)data.GetValue(4);
        card.health = (int)data.GetValue(5);
        card.attack = (int)data.GetValue(6);
        card.SetSprite();
        bool isNft = (bool)data.GetValue(7);
        bool flag = (bool)data.GetValue(8);

        if (PV.IsMine)
        {
            transform.position = cardSpawnPoint.position;
            (isNft ? _GameManager.Inst.myNftCards : _GameManager.Inst.myNormalCards).Add(this);
        }
        else
        {
            transform.position = otherCardSpawnPoint.position;
            (isNft ? _GameManager.Inst.otherNftCards : _GameManager.Inst.otherNormalCards).Add(this);
        }
            
        Setup(card, isNft, flag);
    }

    private void OnDestroy()
    {
        bool isNft = (card.type == "NFT") ? true : false;

        if (PV.IsMine)
        {
            if (isNft)
                _GameManager.Inst.myNftCards.Remove(this);
            else
                _GameManager.Inst.myNormalCards.Remove(this); 
        }
        else
        {
            if(isNft)
                _GameManager.Inst.otherNftCards.Remove(this);
            else
                _GameManager.Inst.otherNormalCards.Remove(this);
        }
        transform.DOKill();
    }

    public void Setup(Card card, bool isNft, bool flag)
    {
        this.card = card;
        isFront = PV.IsMine;

        if (isFront || card.type == "NFT")
        {
            if (card.type == "NFT")
                cardFrame.sprite = Resources.Load("Prefabs/CardGameProject_jpg/NFTCardFrame", typeof(Sprite)) as Sprite;
            else
                cardFrame.sprite = Resources.Load("Prefabs/CardGameProject_jpg/CardFrame", typeof(Sprite)) as Sprite;
            character.sprite = this.card.sprite;
            nameTMP.text = this.card.name;
            attackTMP.text = this.card.attack.ToString();
            healthTMP.text = this.card.health.ToString();
            costTMP.text = this.card.cost.ToString();
        }
        else
        {
            cardFrame.sprite = cardBack;
            character.sprite = null;
            nameTMP.text = "";
            attackTMP.text = "";
            healthTMP.text = "";
            costTMP.text = "";
        }

        StartCoroutine(Setting(isNft, flag, this));
    }


    IEnumerator Setting(bool isNft, bool flag, CardPrefab cardPrefab)
    {
        SetOriginOrder(isNft);

        if (flag)
        {
            ShowCard(cardPrefab);
            yield return new WaitForSeconds(1.5f);
        }

        if ( PV.IsMine && _GameManager.Inst.myNormalCards.Count > 7)
        {   
            Debug.Log("나 : 이미 7장의 노말 카드를 들고 있습니다. 카드가 파괴됩니다.");
            MessageManager.Inst.SetMessage("노말 카드는 최대 7장까지 손에 들고 있을 수 있습니다. 이 카드는 파괴됩니다.");
            _GameManager.Inst.myNormalCards.Remove(cardPrefab);
            Managers.Resource.DestroyImmediate(cardPrefab.gameObject);

        }
        if( !PV.IsMine && _GameManager.Inst.otherNormalCards.Count > 7)
        {
            _GameManager.Inst.myNormalCards.Remove(cardPrefab);
            Managers.Resource.DestroyImmediate(cardPrefab.gameObject);
        }

        CardAlignment(isNft);
    }

    void ShowCard(CardPrefab temp)
    {
        if (PV.IsMine)
        {
            PRS pRS = new PRS(MyShowCardArea.transform.position, MyShowCardArea.transform.rotation, MyShowCardArea.transform.localScale);
            temp.MoveTransform(pRS, true, 0.8f);
        }
        else
        {
            PRS pRS = new PRS(OtherShowCardArea.transform.position, OtherShowCardArea.transform.rotation, OtherShowCardArea.transform.localScale);
            temp.MoveTransform(pRS, true, 0.8f);
        }
    }

    void SetOriginOrder(bool isNft)
    {
        int count = PV.IsMine ? (isNft ? _GameManager.Inst.myNftCards.Count : _GameManager.Inst.myNormalCards.Count) : (isNft ? _GameManager.Inst.otherNftCards.Count : _GameManager.Inst.otherNormalCards.Count);

        for (int i = 0; i < count; i++)
        {
            var targetCard = PV.IsMine ? (isNft ? _GameManager.Inst.myNftCards[i] : _GameManager.Inst.myNormalCards[i]) : (isNft ? _GameManager.Inst.otherNftCards[i] : _GameManager.Inst.otherNormalCards[i]);
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment(bool isNft)
    {
        List<PRS> originCardPRSs;
            
        if (PV.IsMine)
            originCardPRSs =
                RoundAlignment(
                isNft ? NFTMyCardLeft : myCardLeft,
                isNft ? NFTMyCardRight : myCardRight,
                isNft ? _GameManager.Inst.myNftCards.Count : _GameManager.Inst.myNormalCards.Count,
                0.5f,
                Vector3.one * 0.6f
                );
        else
            originCardPRSs =
                RoundAlignment(
                isNft ? NFTOtherCardLeft : otherCardLeft,
                isNft ? NFTOtherCardRight : otherCardRight,
                isNft ? _GameManager.Inst.otherNftCards.Count : _GameManager.Inst.otherNormalCards.Count,
                -0.5f,
                Vector3.one * 0.6f);

        var targetCards = PV.IsMine ? (isNft ? _GameManager.Inst.myNftCards : _GameManager.Inst.myNormalCards) : (isNft ? _GameManager.Inst.otherNftCards : _GameManager.Inst.otherNormalCards);
        for (int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];
            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
    }

    List<PRS> RoundAlignment(
        Transform leftTr,
        Transform rightTr,
        int objCount,
        float height,
        Vector3 scale)
    {
        float[] objLerps = new float[objCount + 1];
        List<PRS> results = new List<PRS>(objCount);


        float interval = 1f / (objCount + 1);
        for (int i = 1; i <= objCount; i++)
            objLerps[i] = interval * i;

        for (int i = 1; i <= objCount; i++)
        {
            var targetPos =
                Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Utill.QI;
            if (objCount >= 4)
            {
                float curve =
                    Mathf
                        .Sqrt(Mathf.Pow(height, 2) -
                        Mathf.Pow(objLerps[i] - 0.5f, 2));
                curve = height >= 0 ? curve : -curve;
                targetPos.y += curve;
                targetPos.y += height >= 0 ? -0.275f : 0.275f;
                targetRot =
                    Quaternion
                        .Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            }
            results.Add(new PRS(targetPos, targetRot, scale));
        }
        return results;
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }

    public void MoveTransform2(bool isEnlarge, PRS prs, bool useDotween, float dotweenTime = 0)
    {
        
        PV.RPC("MoveTransform2RPC", RpcTarget.All, isEnlarge, prs.pos, prs.rot, prs.scale , useDotween, dotweenTime);
    }

    [PunRPC]
    public void MoveTransform2RPC(bool isEnlarge, Vector3 pos, Quaternion rot, Vector3 scale, bool useDotween, float dotweenTime = 0)
    {
        GetComponent<Order>().SetMostFrontOrder(isEnlarge);

        PRS prs = new PRS(pos, rot, scale);
        if (PV.IsMine)
        {
            if (useDotween)
            {
                transform.DOMove(prs.pos, dotweenTime);
                transform.DORotateQuaternion(prs.rot, dotweenTime);
                transform.DOScale(prs.scale, dotweenTime);
            }
            else
            {
                transform.position = prs.pos;
                transform.rotation = prs.rot;
                transform.localScale = prs.scale;
            }
        }
        else
        {
            Vector3 temp = new Vector3(originPRS.pos.x, originPRS.pos.y, prs.pos.z);
            prs = new PRS(temp, prs.rot, prs.scale);
            if (useDotween)
            {
                transform.DOMove(prs.pos, dotweenTime);
                transform.DORotateQuaternion(prs.rot, dotweenTime);
                transform.DOScale(prs.scale, dotweenTime);
            }
            else
            {
                transform.position = prs.pos;
                transform.rotation = prs.rot;
                transform.localScale = prs.scale;
            }
        }
    }

    public void MoveTransform3(PRS prs, bool useDotween, float dotweenTime = 0)
    {

        PV.RPC("MoveTransform3RPC", RpcTarget.All, prs.pos, prs.rot, prs.scale, useDotween, dotweenTime);
    }
    [PunRPC]
    public void MoveTransform3RPC(Vector3 pos, Quaternion rot, Vector3 scale, bool useDotween, float dotweenTime = 0)
    {
        PRS prs = new PRS(pos, rot, scale);

        if (PV.IsMine)
        {
            if (useDotween)
            {
                transform.DOMove(prs.pos, dotweenTime);
                transform.DORotateQuaternion(prs.rot, dotweenTime);
                transform.DOScale(prs.scale, dotweenTime);
            }
            else
            {
                transform.position = prs.pos;
                transform.rotation = prs.rot;
                transform.localScale = prs.scale;
            }
        }
        else
        {
            Vector3 temp = new Vector3(-prs.pos.x, -prs.pos.y, prs.pos.z);
            prs = new PRS(temp, prs.rot, prs.scale);
            if (useDotween)
            {
                transform.DOMove(prs.pos, dotweenTime);
                transform.DORotateQuaternion(prs.rot, dotweenTime);
                transform.DOScale(prs.scale, dotweenTime);
            }
            else
            {
                transform.position = prs.pos;
                transform.rotation = prs.rot;
                transform.localScale = prs.scale;
            }
        }
    }

    void OnMouseOver()
    {
        if (PV.IsMine)
            _GameManager.Inst.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        if (PV.IsMine)
            _GameManager.Inst.CardMouseExit(this);
    }

    void OnMouseDown()
    {
        if (PV.IsMine)
            _GameManager.Inst.CardMouseDown();
    }

    void OnMouseUp()
    {
        if (PV.IsMine)
            _GameManager.Inst.CardMouseUp();
    }

}
