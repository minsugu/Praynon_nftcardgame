using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;

    void Start()
    {

    }

    protected virtual void Init()
    {
        GameObject go = GameObject.Find("@NetworkManager");
        if (go == null)
        {
            go = Managers.Resource.Instantiate("@NetworkManager");
            DontDestroyOnLoad(go);
        }

        Object obj = FindObjectOfType(typeof(EventSystem));
        if (obj == null)
        {
            obj = Managers.Resource.Instantiate("UI/EventSystem");
            DontDestroyOnLoad(obj);
        }
            
    }

    public abstract void Clear();
}
