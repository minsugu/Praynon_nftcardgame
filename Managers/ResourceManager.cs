using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager 
{
    public T Load<T>(string path) where T : Object
    {
        //1. original을 들고 있으면 바로 사용
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }

        return Resources.Load<T>(path);
    }
    
    public GameObject Instantiate(string path, Transform parent = null)
    {
        
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if(original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        //2. 풀링 가능한 아이였다면 pop처리 
        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original, parent).gameObject;


        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;

        return go; 
    }

    public GameObject Instantiate(GameObject _go, Vector3 spawnPos, Quaternion spawnRot)
    {
        GameObject go = Object.Instantiate(_go, spawnPos, spawnRot);
        go.name = _go.name;

        return go;
    }

    public GameObject Instantiate(GameObject _go)
    {
        GameObject go = Object.Instantiate(_go);
        go.name = _go.name;

        return go;
    }

    public void Destroy(GameObject go)
    {
        if(go == null)
            return;

        // 3. 혹시 풀링이 필요한 아이라면 -> 풀링 매니저한테 위탁
        Poolable poolable = go.GetComponent<Poolable>(); 

        if(poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        } //
       
        Object.Destroy(go);
    }

    public void DestroyImmediate(GameObject go)
    {
        if (go == null)
            return;

        // 3. 혹시 풀링이 필요한 아이라면 -> 풀링 매니저한테 위탁
        Poolable poolable = go.GetComponent<Poolable>();

        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        } //

        Object.DestroyImmediate(go);
    }
}
