using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance; 
    public static Managers Instance { get { Init(); return s_instance; } } // 유일성이 보장된다. 싱글톤 패턴

    InputManager _input = new InputManager();
    ResourceManager _resource = new ResourceManager();
    UIManager _ui = new UIManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    PoolManager _pool = new PoolManager();
    DataManager _data = new DataManager();
 
    public static InputManager Input { get { return Instance._input; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static DataManager Data { get { return Instance._data; } }

    void Start()
    {
        Init();
    }

    void Update()
    {
        _input.OnUpdate();
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject{ name = "@Managers"};
                go.AddComponent<Managers>();
            }
            
            if(go != null) 
                DontDestroyOnLoad(go);

            s_instance = go.GetComponent<Managers>();
            s_instance._data.Init();
            s_instance._sound.Init();
            s_instance._pool.Init();

        }
       
    }

    public static void Clear()
    {
        Input.Clear();
        Scene.Clear();
        Pool.Clear();
    }
}
