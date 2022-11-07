using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 치트, UI, 랭킹, 게임오버
public class _GameManager : MonoBehaviourPunCallbacks
{
    public static _GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    PhotonView PV;

    WaitForSeconds delay1 = new WaitForSeconds(1);
    WaitForSeconds delay2 = new WaitForSeconds(2);
    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);
    WaitForSeconds delay09 = new WaitForSeconds(0.9f);


    #region GameManager Properties
    [Header("GameManager")]
    [Multiline(10)]
    [SerializeField] string cheatInfo;
    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] TitlePanel titlePanel;
    [SerializeField] CameraController cameraController;
    [SerializeField] GameObject endTurnBtn;
    #endregion

    #region TurnManager Properties
    [Header("TurnManager")]
    [SerializeField] [Tooltip("시작 턴 모드를 정합니다")] ETurnMode eTurnMode;
    [SerializeField] [Tooltip("카드 배분이 매우 빨라집니다")] bool fastMode;
    [SerializeField] [Tooltip("시작 카드 개수를 정합니다")]
    int startCardCount;
    public bool isGaming;
    public bool isLoading; // 게임 끝나면 isLoading이 true -> 카드, 엔티티 클릭 x  
    public bool myTurn; // 내 턴?
    enum ETurnMode
    {
        Random,
        My,
        Other,
    }
    public static event Action<bool> OnTurnStarted;
    #endregion

    #region CardManager Properties
    [Header("CardManager")]
    [SerializeField] public List<CardPrefab> myNormalCards;
    [SerializeField] public List<CardPrefab> myNftCards;
    [SerializeField] public Transform cardSpawnPoint;
    [SerializeField] public Transform myCardLeft;
    [SerializeField] public Transform myCardRight;
    [SerializeField] public Transform NFTMyCardLeft;
    [SerializeField] public Transform NFTMyCardRight;
    [SerializeField] public Transform MyShowCardArea;

    [SerializeField] public List<CardPrefab> otherNormalCards;
    [SerializeField] public List<CardPrefab> otherNftCards;
    [SerializeField] public Transform otherCardSpawnPoint;
    [SerializeField] public Transform otherCardLeft;
    [SerializeField] public Transform otherCardRight;
    [SerializeField] public Transform NFTOtherCardLeft;
    [SerializeField] public Transform NFTOtherCardRight;
    [SerializeField] public Transform OtherShowCardArea;

    [SerializeField] GameObject prefabForCard;
    [SerializeField] ECardState eCardState;
    enum ECardState
    {
        Nothing,
        CanMouseOver,
        CanMouseDrag
    }
    public CardPrefab selectCard;
    public List<Card> myNormalTemp;
    public List<Card> myNftTemp;

    public List<Card> otherNormalTemp;
    public List<Card> otherNftTemp;

    public bool isMyCardDrag;
    public bool onMyCardArea;
    public int myBossDrainCnt;
    public int otherBossDrainCnt;
    #endregion

    #region EntityManager Properties
    [Header("EntityManager")]
    [SerializeField] public List<Entity> myEntities;
    [SerializeField] public List<Entity> otherEntities;
    [SerializeField] GameObject entityPrefab;
    [SerializeField] public GameObject attackPrefab;
    [SerializeField] public GameObject defendPrefab;
    [SerializeField] public GameObject TargetPicker;
    [SerializeField] Entity myEmptyEntity;
    [SerializeField] public Entity myBossEntity;
    [SerializeField] Entity otherEmptyEntity;
    [SerializeField] public Entity otherBossEntity;

    public const int MAX_ENTITY_COUNT = 5;
    public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    public bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    public bool ExistTargetPickEntity => targetPickEntity != null;

    public bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);
    public int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);
    public bool ExistOtherEmptyEntity => otherEntities.Exists(x => x == otherEmptyEntity);
    public int OtherEmptyEntityIndex => otherEntities.FindIndex(x => x == otherEmptyEntity);

    public bool CanMouseInput => myTurn && !isLoading;

    Entity selectEntity;
    Entity targetPickEntity;
    #endregion

    #region ManaManager Properties
    [Header("ManaSettings")]
    [SerializeField] public Transform myGridSetting;
    [SerializeField] public Transform otherGridSetting;
    #endregion


    void Start()
    {
        PV = GetComponent<PhotonView>();

        UISetup();
        CardSetUp();
        myBossDrainCnt = 0;
        otherBossDrainCnt = 0;
        isGaming = false;
    }

    void Update()
    {
        if (!isGaming)
            return;

        #if UNITY_EDITOR
                InputCheatKey();
        #endif

        if (isMyCardDrag) CardDrag();

        DetectCardArea();
        SetECardState();
        ShowTargetPicker(ExistTargetPickEntity);
    }

    void OnDestroy()
    {

    }

    void UISetup()
    {
        notificationPanel.ScaleZero();
        resultPanel.ScaleZero();
        titlePanel.Active(true);
        cameraController.SetGrayScale(false);
    }

    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            AddCard(false, true);

        if (Input.GetKeyDown(KeyCode.Keypad2))
            EndTurn();

        if (Input.GetKeyDown(KeyCode.Keypad3))
            TryPutCard(false);


        if (Input.GetKeyDown(KeyCode.Keypad6))
            ManaManager.Inst.AddMana(3);

    }


    #region GameManager
    public bool StartGame() 
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            MessageManager.Inst.SetMessage("다른 플레이어를 기다리세요");
            return false;
        }
        eTurnMode = ETurnMode.My; // 시작 버튼 누른사람이 선공 가져가기..
        PV.RPC("StartGameCoRPC", RpcTarget.AllViaServer);
        return true;
    }

    [PunRPC]
    public void StartGameCoRPC()
    {
        isGaming = true;
        StartCoroutine(StartGameCo());
    }


    public void GameOver()
    {
        PV.RPC("GameOverCoRPC", RpcTarget.Others);
        StartCoroutine(GameOver(false));
    }


    public IEnumerator GameOver(bool isMyWin)
    {
        isGaming = false;
        isLoading = true;
        endTurnBtn.SetActive(false);
        yield return delay1;

        isLoading = true;
        resultPanel.Show(isMyWin ? "Victory" : "Defeat");
        if (isMyWin)
            resultPanel.SetBlueColor();
        else
            resultPanel.SetRedColor();
        cameraController.SetGrayScale(true);
        yield return new WaitForSeconds(6000);
    }

    [PunRPC]
    public void GameOverCoRPC()
    {
        Debug.Log("GameOverCoRPC");
        StartCoroutine(GameOverCo(true));
    }
    public IEnumerator GameOverCo(bool isMyWin)
    {
        Debug.Log("GameOverCo");
        isGaming = false;
        isLoading = true;
        endTurnBtn.SetActive(false);
        yield return delay1;

        isLoading = true;
        resultPanel.Show(isMyWin ? "Victory" : "Defeat");
        if (isMyWin)
            resultPanel.SetBlueColor();
        else
            resultPanel.SetRedColor();
        cameraController.SetGrayScale(true);
        yield return new WaitForSeconds(6000);
    }
    public void Notification(string message) => notificationPanel.Show(message);
    #endregion

    #region TurnManager

    public void GameSetup()
    {
        AddCardsToTemp();
        switch (eTurnMode)
        {
            case ETurnMode.Random:
                myTurn = UnityEngine.Random.Range(0, 2) == 0;
                break;
            case ETurnMode.My:
                myTurn = true;
                break;
            case ETurnMode.Other:
                myTurn = false;
                break;
        }
        if (fastMode) delay05 = new WaitForSeconds(0.075f);
    }

    public IEnumerator StartGameCo()
    {
        Debug.Log("StartGameCo 동작");

        GameSetup();
        isLoading = true;
       
        for(int i=0; i< 3; i++)
        {
            yield return delay05;
            AddCard(true , false);
        }
        for (int i = 3; i < startCardCount; i++)
        {
            yield return delay05;
            AddCard(false , false);
        }
        yield return delay1;

        if (myTurn)
            StartCoroutine(StartTurnCo());
        else
            StartCoroutine(WaitMyTurn());
    }

    IEnumerator WaitMyTurn() 
    {
        isLoading = true;
        OnTurnStarted?.Invoke(false);
        AttackableReset(false);
        yield return new WaitForSeconds (6000f);

    }

    IEnumerator StartTurnCo()
    {
        isLoading = true;
        Notification("MY TURN");

        yield return delay09;
        yield return delay09;
        AddCard(false, true);
        ManaManager.Inst.InitMana();
        yield return delay07;
        isLoading = false;

        OnTurnStarted?.Invoke(true);
        AttackableReset(true); // from EntityManager
    }

    public void EndTurn() => PV.RPC("EndTurnRPC", RpcTarget.All);
    [PunRPC]
    public void EndTurnRPC()
    {
        Debug.Log("TurnChanged!!");
        myTurn = !myTurn;
        if (myTurn)
            StartCoroutine(StartTurnCo());
        else
            StartCoroutine(WaitMyTurn());
    }

    #endregion

    #region CardManager

    #region Card
    void CardSetUp()
    {
        foreach (Card _card in Managers.Data.NftCardDict.Values)
        {
            Managers.Data.PushCardToBuffer(_card);
        }
        foreach (Card _card in Managers.Data.CardDict.Values)
        {
            Managers.Data.PushCardToBuffer(_card);
        }
    }

    public void AddCardsToTemp()
    {
        myNormalTemp = new List<Card>();
        for (int i = 0; i < Managers.Data.GetNormalCardBuffer().Count; i++)
        {
            myNormalTemp.Add(Managers.Data.GetNormalCardBuffer()[i]);
        }
        myNftTemp = new List<Card>();
        for (int i = 0; i < Managers.Data.GetNftCardBuffer().Count; i++)
        {
            myNftTemp.Add(Managers.Data.GetNftCardBuffer()[i]);
        }
    }
    #endregion

    #region Game

    public Card PopCard(bool isNft)
    {
        Card card;
        if (isNft)
        {
            if (myNftTemp.Count == 0)
            {
                Debug.Log($"<MyPlayer> : 뽑을 수 있는 NFT 카드가 없습니다.");
                //MessageManager.Inst.SetMessage("There isn't any NFT Card in your deck..");
                return null;
            }
            card = myNftTemp[0];
            myNftTemp.RemoveAt(0);
        }
        else
        {
            if (myNormalTemp.Count == 0)
            {
                myBossDrainCnt++;
                Debug.Log($"<MyPlayer> : 덱이 모두 소진되었습니다. {myBossDrainCnt}만큼 데미지를 입습니다.");
                MessageManager.Inst.SetMessage($"더 이상 뽑을 수 있는 카드가 없습니다, {myBossDrainCnt} 만큼 플레이어가 데미지를 입습니다!");
                if (!myBossEntity.Damaged(myBossDrainCnt))
                {
                    Managers.Sound.Play("KillBoss", Define.Sound.Effect);
                    GameOver();
                }
                return null;
            }
            int _index = UnityEngine.Random.Range(0, myNormalTemp.Count); // 랜덤으로 노말 카드 뽑기
            card = myNormalTemp[_index];
            myNormalTemp.RemoveAt(_index);
        }

        Managers.Sound.Play("CardSlide", Define.Sound.Effect);
        return card;
    }

    void AddCard(bool isNft, bool flag)
    {
        Card card = PopCard(isNft);
        if (card == null)
            return;

        object[] data = { card.no, card.type, card.name, card.explain, card.cost, card.health, card.attack, isNft, flag };
        PhotonNetwork.Instantiate("card", cardSpawnPoint.position, Utill.QI, 0, data);
    }

 
    public void CardAlignment(bool isNft)
    {
        List<PRS> originCardPRSs;

        originCardPRSs =
            RoundAlignment(
            isNft ? NFTMyCardLeft : myCardLeft,
            isNft ? NFTMyCardRight : myCardRight,
            isNft ? myNftCards.Count : myNormalCards.Count,
            0.5f,
            Vector3.one * 0.6f
            );

        var targetCards = (isNft ? myNftCards : myNormalCards);
        for (int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];
            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
    }

    [PunRPC]
    public void CardAlignmentRPC(bool isNft)
    {
        List<PRS> originCardPRSs;
    
        originCardPRSs =
            RoundAlignment(
            isNft ? NFTOtherCardLeft : otherCardLeft,
            isNft ? NFTOtherCardRight : otherCardRight,
            isNft ? otherNftCards.Count : otherNormalCards.Count,
            -0.5f,
            Vector3.one * 0.6f);

        var targetCards = isNft ? otherNftCards : otherNormalCards;
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

    public bool TryPutCard(bool isNft)
    {
        CardPrefab cardPrefab;
        Vector3 spawnPos;
        List<CardPrefab> targetCards;

        cardPrefab = selectCard;
        spawnPos = Utill.MousePos;
        targetCards = isNft ? myNftCards : myNormalCards;

        if (SpawnEntity(cardPrefab.card, spawnPos))
        {
            selectCard = null;
            PhotonNetwork.Destroy(cardPrefab.GetComponent<PhotonView>());
            CardAlignment(isNft);
            PV.RPC("CardAlignmentRPC", RpcTarget.Others, isNft);
        }
        else
        {
            targetCards.ForEach(x => x.GetComponent<Order>().SetMostFrontOrder(false));
            CardAlignment(isNft);
            PV.RPC("CardAlignmentRPC", RpcTarget.All, isNft);
            return false;
        }
        Managers.Sound.Play("cardPlace", Define.Sound.Effect);

        return true;
    }
    #endregion

    #region MyCard
    public void CardMouseOver(CardPrefab cardPrefab)
    {
        if (eCardState == ECardState.Nothing) return;

        selectCard = cardPrefab;
        EnlargeCard(true, cardPrefab);
    }

    public void CardMouseExit(CardPrefab cardPrefab)
    {
        EnlargeCard(false, cardPrefab);
    }

    public void CardMouseDown()
    {
        if (eCardState != ECardState.CanMouseDrag) return;

        isMyCardDrag = true;
    }

    public void CardMouseUp()
    {
        isMyCardDrag = false;
        if (eCardState != ECardState.CanMouseDrag) return;

        if (onMyCardArea)
        {
            RemoveMyEmptyEntity();
        }
        else
        {
            int _cost = selectCard.card.cost;
            string _type = selectCard.card.type;

            if (_cost > ManaManager.Inst.myManaCnt)
            {
                Debug.Log("나 : 마나가 부족합니다");
                MessageManager.Inst.SetMessage("마나가 부족합니다..");
                return;
            }

            if (TryPutCard(_type == "NFT" ? true : false))
                ManaManager.Inst.UseMana(_cost);
        }

    }

    void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag) return;

        if (!onMyCardArea)
        {
            selectCard.MoveTransform3(new PRS(Utill.MousePos, Utill.QI, selectCard.originPRS.scale), false);
            InsertMyEmptyEntity(Utill.MousePos.x);
        }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utill.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    void EnlargeCard(bool isEnlarge, CardPrefab cardPrefab)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(cardPrefab.originPRS.pos.x, -2.8f, -10f);
            cardPrefab.MoveTransform2(isEnlarge, new PRS(enlargePos, Utill.QI, Vector3.one * 1.2f), false);
        }
        else
            cardPrefab.MoveTransform2(isEnlarge, cardPrefab.originPRS, false);
    }

    void SetECardState()
    {
        if (isLoading)
            eCardState = ECardState.Nothing;
        else if (!myTurn || IsFullMyEntities)
            eCardState = ECardState.CanMouseOver;
        else if (myTurn)
            eCardState = ECardState.CanMouseDrag;
    }
    #endregion

    #endregion

    #region EntityManager

    #region Entity 생성, 삭제, 정렬
    void EntityAlignment(bool isMine)
    {
        float targetY = isMine ? -1.4f : 1.4f;
        var targetEntities = isMine ? myEntities : otherEntities;

        for (int i = 0; i < targetEntities.Count; i++)
        {
            float targetX = (targetEntities.Count - 1) * -1.035f + i * 2f;
            targetX = isMine ? targetX : -targetX;

            var targetEntity = targetEntities[i];
            targetEntity.originPos = new Vector3(targetX, targetY, 0);
            targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f);
            targetEntity.GetComponent<Order>()?.SetOriginOrder(i);
        }
    }

    
    public void RemoveMyEmptyEntity()
    {
        if (!ExistMyEmptyEntity)
            return;

        myEntities.RemoveAt(MyEmptyEntityIndex);
        EntityAlignment(true);
        PV.RPC("RemoveMyEmptyEntityRPC", RpcTarget.Others);
    }

    [PunRPC]
    public void RemoveMyEmptyEntityRPC()
    {
        if (!ExistOtherEmptyEntity)
            return;

        otherEntities.RemoveAt(OtherEmptyEntityIndex);
        EntityAlignment(false);
    }


    public void InsertMyEmptyEntity(float xPos)
    {
        if (IsFullMyEntities)
        {
            return;
        }
            

        if (!ExistMyEmptyEntity)
            myEntities.Add(myEmptyEntity);

        Vector3 emptyEntityPos = myEmptyEntity.transform.position;
        emptyEntityPos.x = xPos;
        myEmptyEntity.transform.position = emptyEntityPos;

        int _emptyEntityIndex = MyEmptyEntityIndex;
        myEntities.Sort((entity1, entity2) => entity1.transform.position.x.CompareTo(entity2.transform.position.x));

        if (MyEmptyEntityIndex != _emptyEntityIndex)
            EntityAlignment(true);

        PV.RPC("InsertOtherEmptyEntityRPC", RpcTarget.Others, xPos);
    }

    [PunRPC]
    public void InsertOtherEmptyEntityRPC(float xPos)
    {
        if (IsFullOtherEntities)
            return;

        if (!ExistOtherEmptyEntity)
            otherEntities.Add(otherEmptyEntity);

        Vector3 emptyEntityPos = otherEmptyEntity.transform.position;
        emptyEntityPos.x = -xPos;
        emptyEntityPos.y = otherEmptyEntity.transform.position.y;
        otherEmptyEntity.transform.position = emptyEntityPos;

        int _emptyEntityIndex = OtherEmptyEntityIndex;
        otherEntities.Sort((entity1, entity2) => (-entity1.transform.position.x).CompareTo(-entity2.transform.position.x));

        if (OtherEmptyEntityIndex != _emptyEntityIndex)
            EntityAlignment(false);
    }

    public bool SpawnEntity(Card card, Vector3 spawnPos)
    {
        if (IsFullMyEntities || !ExistMyEmptyEntity)
            return false;

        object[] data = { card.no, card.type, card.name, card.explain, card.cost, card.health, card.attack };
        PhotonNetwork.Instantiate("Entity", spawnPos, Utill.QI, 0, data);

        return true;
    }
    #endregion

    #region Entity(보스) 공격, 데미지 처리
    public void EntityMouseDown(Entity entity)
    {
        if (!CanMouseInput)
            return;

        selectEntity = entity;
        
    }

    public void EntityMouseUp()
    {
        if (!CanMouseInput)
            return;

        // selectEntity, targetPickEntity 둘다 존재하면 공격한다. 바로 null, null로 만든다.
        if (selectEntity && targetPickEntity && selectEntity.attackable)
            selectEntity.Attack(targetPickEntity);

        selectEntity = null;
        targetPickEntity = null;
    }

    public void EntityMouseDrag()
    {
        if (!CanMouseInput || selectEntity == null)
            return;

        // other 타겟엔티티 찾기
        bool existTarget = false;
        foreach (var hit in Physics2D.RaycastAll(Utill.MousePos, Vector3.forward))
        {
            Entity entity = hit.collider?.GetComponent<Entity>();
            if (entity != null && !entity.isMine && selectEntity.attackable)
            {
                targetPickEntity = entity;
                existTarget = true;
                break;
            }
        }
        if (!existTarget)
            targetPickEntity = null;
    }


    void ShowTargetPicker(bool isShow)
    {
        TargetPicker.SetActive(isShow);
        if (ExistTargetPickEntity)
        {
            TargetPicker.transform.position = targetPickEntity.transform.position;
        }
    }

    public void AttackableReset(bool isMine)
    {
        var targetEntites = isMine ? myEntities : otherEntities;
        targetEntites.ForEach(x => x.attackable = true);
    }

    #endregion

    #endregion
}
