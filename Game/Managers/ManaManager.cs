using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaManager : UI_Base
{
    public static ManaManager Inst { get; private set; }
    void Awake() => Inst = this;

    Stack<Mana> myManaStack;
    Stack<Mana> otherManaStack;

    public int  myManaCnt = 0;
    public int myManaMax = 0;
    public int otherManaCnt = 0;
    public int otherManaMax = 0;

    public static Action<int, bool> OnAddMana;
    public static event Action<int, bool> OnUseMana;
    public static event Action<int, bool> OnInitMana;

    public PhotonView PV;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        myManaStack = new Stack<Mana>();
        otherManaStack = new Stack<Mana>();
        PV= GetComponent<PhotonView>();
    }


    public void InitMana()
    {
        if (myManaMax < 10)
            myManaMax++;
        OnInitMana.Invoke(myManaMax, true);

        PV.RPC("otherInitMana", RpcTarget.Others);

        StartCoroutine(ManaSetting());
    }

    IEnumerator ManaSetting()
    {
        UseMana(myManaCnt);
        yield return new WaitForSeconds(0.8f);
        AddMana(myManaMax - myManaCnt);
       
    }

    public bool UseMana(int k)
    {
        if (k > myManaCnt)
        {
            Debug.Log("나 : 마나가 부족합니다");
            MessageManager.Inst.SetMessage("마나가 부족합니다..");
            return false;
        }
        else
        {
            for (int i = 0; i < k; i++)
            {
                Mana mana = myManaStack.Pop();
                Managers.Resource.DestroyImmediate(mana.gameObject);
                myManaCnt--;
            }
            Debug.Log($"나 : 마나를 {k}만큼 사용하였습니다.");
            OnUseMana.Invoke(myManaCnt, true);
        }
        PV.RPC("otherUseMana", RpcTarget.Others, k);
        return true;
    }

    public void AddMana(int k)
    {
        Debug.Log($"My : AddMana {k}");

        for (int i = 0; i < k; i++)
        {
            if (myManaCnt >= 10)
            {
                Debug.Log("나 : 마나가 10까지 가득 찼습니다");
                break;
            }
            Mana mana = Managers.UI.MakeSubItem<Mana>(GameObject.Find($"MySlot{myManaCnt}").transform);
            myManaStack.Push(mana);
            myManaCnt++;
        }
        Debug.Log($"나 : 마나를 {k} 만큼 추가합니다");
        OnAddMana.Invoke(myManaCnt, true);
        
        PV.RPC("otherAddMana", RpcTarget.Others, k);
    }




    [PunRPC]
    public void otherInitMana()
    {
        if (otherManaMax < 10)
            otherManaMax++;
        OnInitMana.Invoke(otherManaMax, false);
    }

    [PunRPC]
    public void otherUseMana(int k)
    {
        OnUseMana.Invoke(k, false);
        for (int i = 0; i < k; i++)
        {
            Mana mana = otherManaStack.Pop();
            Managers.Resource.DestroyImmediate(mana.gameObject);
            otherManaCnt--;
        }
        Debug.Log($"Enemy : 마나를 {k}만큼 사용하였습니다.");
        OnUseMana.Invoke(otherManaCnt, false);
    }

    [PunRPC]
    public void otherAddMana(int k)
    {
        Debug.Log($"Enemy : AddMana {k}");

        for (int i = 0; i < k; i++)
        {
            if (otherManaCnt >= 10)
            {
                Debug.Log("Enemy : 마나가 10까지 가득 찼습니다");
                break;
            }
            Mana mana = Managers.UI.MakeSubItem<Mana>(GameObject.Find($"OtherSlot{otherManaCnt}").transform);
            otherManaStack.Push(mana);
            otherManaCnt++;
        }
        Debug.Log($"Enemy : 마나를 {k}만큼 추가합니다");
        OnAddMana.Invoke(otherManaCnt, false);
    }




}
