using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T:MonoSingleton<T>
{
    protected static T instance;

    public static T Instance
    {
        get { return instance; }
    }

    protected virtual void Awake()
    {
        if (instance == null)
            instance = (T)this;
        else
            Debug.LogError("该mono Class 脚本已存在，不可重复添加! "+ this.GetType());
    }
}
