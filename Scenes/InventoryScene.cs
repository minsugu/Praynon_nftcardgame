using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScene : BaseScene
{
    private void Start()
    {
        Init();
    }

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Inventory;
    }

    public override void Clear()
    {
        Debug.Log("InventorySceneClear!!");
    }
}
