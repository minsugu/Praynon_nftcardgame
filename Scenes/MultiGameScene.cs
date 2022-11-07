using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiGameScene : BaseScene
{
    private void Start()
    {
        Init();
    }

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.MultiGame; 
        Managers.Sound.Play("GameScene", Define.Sound.Bgm);
    }

    public override void Clear()
    {
        Debug.Log("MultiGameSceneClear!!");
        Managers.Sound.Play("MainTrack", Define.Sound.Bgm, 0.9f);
    }
}
