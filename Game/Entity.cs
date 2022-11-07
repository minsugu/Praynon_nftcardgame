using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Photon.Pun;

public class Entity : MonoBehaviour
{
    [SerializeField] Card card;
    [SerializeField] SpriteRenderer entity;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] TMP_Text costTMP;
    [SerializeField] GameObject sleepParticle;

    public string name;
    public int attack;
    public int health;
    public int cost;
    public bool isMine;
    public bool isDie;
    public bool isBossOrEmpty;
    public bool attackable;
    public Vector3 originPos;
    int liveCount;

    PhotonView PV;

    void Awake()
    {
        if (isBossOrEmpty)
            return;
        _GameManager.OnTurnStarted += OnTurnStarted;
        
        PV = GetComponent<PhotonView>();
        transform.localScale *= 0.80f;

        if(!PV.IsMine)
            transform.position = 
                new Vector3(-transform.position.x, -transform.position.y, transform.position.z);  


        if (PV.IsMine)
        {
            _GameManager.Inst.myEntities[_GameManager.Inst.MyEmptyEntityIndex] = this;
            isMine = true;
        }
            
        else
        {
            _GameManager.Inst.otherEntities[_GameManager.Inst.OtherEmptyEntityIndex] = this;
            isMine = false;
        }

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

        Setup(card);
        EntityAlignment(PV.IsMine);
    }

    void OnDestroy()
    {
        _GameManager.OnTurnStarted -= OnTurnStarted;
    }


    void OnTurnStarted(bool myTurn)
    {
        if (isBossOrEmpty)
            return;

        if (isMine == myTurn)
            liveCount++;

        sleepParticle.SetActive(liveCount < 1);
    }

    public void Setup(Card card)
    {
        attack = card.attack;
        health = card.health;
        cost = card.cost;
        name = card.name;

        this.card = card;
        if (card.type == "NFT")
            entity.sprite = Resources.Load("Prefabs/CardGameProject_jpg/NFTCardFrame", typeof(Sprite)) as Sprite;
        character.sprite = this.card.sprite;
        nameTMP.text = this.card.name;
        attackTMP.text = attack.ToString();
        healthTMP.text = health.ToString();
        costTMP.text = cost.ToString();
    }

    void EntityAlignment(bool isMine)
    {
        float targetY = isMine ? -1.4f : 1.4f;
        var targetEntities = isMine ? _GameManager.Inst.myEntities : _GameManager.Inst.otherEntities;

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

    public void MoveTransform(Vector3 pos, bool useDotween, float dotweenTime = 0)
    {

        if (useDotween)
            transform.DOMove(pos, dotweenTime);
        else
            transform.position = pos;

    }

    public void MoveTransform2(Vector3 pos, bool useDotween, float dotweenTime = 0)
    {
        PV.RPC("MoveTransformRPC", RpcTarget.All, pos, useDotween, dotweenTime);
    }

    [PunRPC]
    public void MoveTransformRPC(Vector3 pos, bool useDotween, float dotweenTime = 0)
    {
        if (PV.IsMine)
        {
            if (useDotween)
                transform.DOMove(pos, dotweenTime);
            else
                transform.position = pos;
        }
        else
        {
            pos = new Vector3(-pos.x, -pos.y, pos.z);
            if (useDotween)
                transform.DOMove(pos, dotweenTime);
            else
                transform.position = -pos;
        }
    }

    void OnMouseDown()
    {
        if (isMine)
            _GameManager.Inst.EntityMouseDown(this);
    }

    void OnMouseUp()
    {
        if (isMine)
            _GameManager.Inst.EntityMouseUp();
    }

    void OnMouseDrag()
    {
        if (isMine)
            _GameManager.Inst.EntityMouseDrag();
    }


    public bool Damaged(int damage)
    {
        bool isAlive = true;

        health -= damage;
        healthTMP.text = health.ToString();

        if (health <= 0)
        {
            isDie = true;
            SetHp0();
            isAlive = false;
        }
        
        return isAlive;
    }

    public void SetHp0()
    { 
        health = 0;
        healthTMP.text = health.ToString();

    }


    public void Attack(Entity defender)
    {
        if(defender.isBossOrEmpty)
            PV.RPC("AttackRPC", RpcTarget.All, "boss");
        else
            PV.RPC("AttackRPC", RpcTarget.All, defender.name);
    }

    [PunRPC]
    public void AttackRPC(string _name)
    {
        Entity defender = null;
        
        if(_name == "boss")
        {
            defender = PV.IsMine ? _GameManager.Inst.otherBossEntity : _GameManager.Inst.myBossEntity;
            defender.originPos = PV.IsMine ? _GameManager.Inst.otherBossEntity.transform.position : _GameManager.Inst.myBossEntity.transform.position;
        }
            
     

        if (PV.IsMine)
        {
            foreach (Entity ent in _GameManager.Inst.otherEntities)
            {
                if (ent.card.name == _name)
                    defender = ent;
            }
        }
        else
        {
            foreach (Entity ent in _GameManager.Inst.myEntities)
            {
                if (ent.card.name == _name)
                    defender = ent;
            }
        }
        
        
        Debug.Log(defender.card.name);

        attackable = false;
        GetComponent<Order>().SetMostFrontOrder(true);

        Sequence sequence = DOTween.Sequence()
           .Append(transform.DOMove(defender.originPos, 0.4f)).SetEase(Ease.InSine)
           .AppendCallback(() =>
           {
               Damaged(defender.attack);
               defender.Damaged(attack);

               Managers.Sound.Play("Attack", Define.Sound.Effect);

               SpawnDamageAttacker(defender.attack, transform);
               SpawnDamageDefender(attack, defender.transform);
           })
           .Append(transform.DOMove(originPos, 0.4f)).SetEase(Ease.OutSine)
           .OnComplete(() => AttackCallback(this, defender));
    }

    void AttackCallback(params Entity[] entities)
    {
        // Á×À» »ç¶÷ °ñ¶ó¼­ Á×À½ Ã³¸®
        entities[0].GetComponent<Order>().SetMostFrontOrder(false);

        foreach (var entity in entities)
        {
            if (!entity.isDie || entity.isBossOrEmpty)
                continue;

            if (entity.isMine)
                _GameManager.Inst.myEntities.Remove(entity);
            else
                _GameManager.Inst.otherEntities.Remove(entity);

            Managers.Sound.Play("Death", Define.Sound.Effect);

            Sequence sequence = DOTween.Sequence()
                .Append(entity.transform.DOShakePosition(1.3f))
                .Append(entity.transform.DOScale(Vector3.zero, 0.3f)).SetEase(Ease.OutCirc)
                .OnComplete(() =>
                {
                    EntityAlignment(entity.isMine);
                    DestroyImmediate(entity.gameObject);
                });
        }
        StartCoroutine(CheckBossDie());
    }

    public IEnumerator CheckBossDie()
    {
        yield return new WaitForSeconds(1f);

        if (!PV.IsMine && _GameManager.Inst.myBossEntity.isDie)
        {
            _GameManager.Inst.otherBossEntity.SetHp0();
            Managers.Sound.Play("KillBoss", Define.Sound.Effect);
            _GameManager.Inst.GameOver();
        }
        /*
        if(!PV.IsMine && _GameManager.Inst.myBossEntity.isDie)
            PV.RPC("GameOver", RpcTarget.Others);*/
        
    }
    public void GameOver() => _GameManager.Inst.GameOver();



    void SpawnDamageAttacker(int damage, Transform tr)
    {
        if (damage <= 0)
            return;

        Damage damageComponent = Instantiate(_GameManager.Inst.attackPrefab).GetComponent<Damage>();
        damageComponent.SetupTransform(tr);
        damageComponent.Damaged(damage);
    }

    void SpawnDamageDefender(int damage, Transform tr)
    {
        if (damage <= 0)
            return;

        Damage damageComponent = Instantiate(_GameManager.Inst.defendPrefab).GetComponent<Damage>();
        Vector3 temp = new Vector3(0, 3f, 0);
        damageComponent.SetupTransform(tr);
        damageComponent.Damaged(damage);
    }

}
