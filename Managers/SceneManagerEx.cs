using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene{ get { return GameObject.FindObjectOfType<BaseScene>(); } }
    public BaseScene LastScene;

    public void LoadScene(Define.Scene type)
    {
        LastScene = CurrentScene;
        Managers.Clear();
        SceneManager.LoadScene(GetSceneName(type));
    }

    public string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
