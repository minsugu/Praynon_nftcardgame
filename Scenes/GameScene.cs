using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    private void Start()
    {
        Init();
    }

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Game; 
        Managers.Sound.Play("GameScene", Define.Sound.Bgm);
    }

    public override void Clear()
    {
        Managers.Sound.Play("MainTrack", Define.Sound.Bgm, 0.9f);
        Debug.Log("GameSceneClear!!");
    }
}
