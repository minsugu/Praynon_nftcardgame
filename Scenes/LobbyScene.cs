using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : BaseScene 
{ 
    void Start()
    {
        Init();
    }

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Lobby;
    }


    public override void Clear()
    {
        Debug.Log("LobbySceneClear!");
    }
}
