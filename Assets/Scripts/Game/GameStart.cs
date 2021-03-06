﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameStart : MonoSingleton<GameStart>
{
    public RectTransform UIRoot;//UI节点
    public RectTransform WndRoot;//窗口节点
    public Camera UICamera;//UI相机
    public EventSystem EventSystem;//事件系统

    protected override void Awake()
    {
        base.Awake();
        GameObject.DontDestroyOnLoad(gameObject);//切场景不销毁此控制器
        AssetBundleManger.Instance.LoadAssetBundleConfig();//初始化配置表
        ResourceManger.Instance.Init(this);
        ObjectManger.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceenTrs"));
        UIManager.Instance.Init(UIRoot, WndRoot, UICamera, EventSystem);//初始化UI框架
        GameMapManger.Instance.Init(this);//初始化场景加载器
    }
    void Start()
    {
        LoadConfigs();//加载配置表
        RegistUI();//注册UI页面
        PreLoad();///预加载资源放这里
        UIManager.Instance.OpenWnd(ConStr.LOGINPANEL, true);

    }
    /// <summary>
    /// 加载配置表
    /// </summary>
    void LoadConfigs()
    {
        ConfigManger.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
        ConfigManger.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
    }
    /// <summary>
    /// 注册UI窗口
    /// </summary>
    void RegistUI()
    {
        UIManager.Instance.Register<LoginWindow>(ConStr.LOGINPANEL);
        UIManager.Instance.Register<loadWindow>(ConStr.LOADPANEL);
        UIManager.Instance.Register<RegisterWindow>(ConStr.REGISTERPANEL);
        UIManager.Instance.Register<MenuWindow>(ConStr.MENUPANEL);
        UIManager.Instance.Register<InfoWindow>(ConStr.INFOPANEL);
        UIManager.Instance.Register<HelpWindow>(ConStr.HELPPANEL);
        UIManager.Instance.Register<ExitWindow>(ConStr.EXITPANEL);
        UIManager.Instance.Register<SettingWindow>(ConStr.SETTINGPANEL);
        UIManager.Instance.Register<AlertWindow>(ConStr.ALERTPANEL);
        UIManager.Instance.Register<EmbattleWindow>(ConStr.EMBATTLEPANEL);
        UIManager.Instance.Register<SelectWindow>(ConStr.SELECTPANEL);
        UIManager.Instance.Register<HUDWindow>(ConStr.HUDPANEL);
        UIManager.Instance.Register<LoseWindow>(ConStr.LOSEPANEL);
        UIManager.Instance.Register<WinWindow>(ConStr.WINPANEL);
    }

    public void PreLoad()
    {
        ObjectManger.Instance.PreLoadGameObject(ConStr.ALERTPRE);
    }

    void Update()
    {
        UIManager.Instance.OnUpdate();
    }

    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        if(SocketClient.IsOnline)
            SocketClient.Instance.m_Socket.Close();
        ResourceManger.Instance.ClearCache();
        Resources.UnloadUnusedAssets();//编辑器下卸载所有未使用的资源
        Debug.Log("应用退出，清空编辑器缓存");
#endif
    }
}


#region objMgr的使用
//public class GameStart : MonoBehaviour
//{
//    private GameObject go = null;

//    void Awake()
//    {
//        GameObject.DontDestroyOnLoad(gameObject);//切场景不销毁此控制器
//        AssetBundleManger.Instance.LoadAssetBundleConfig();//初始化配置表
//        ResourceManger.Instance.Init(this);
//        ObjectManger.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceenTrs"));
//    }
//	void Start ()
//	{
//        //go = ObjectManger.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab",true);//同步加载
//	    //ObjectManger.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab",OnCallBack,LoadResPriority.RES_HIGH,true);//异步加载
//	    ObjectManger.Instance.PreLoadGameObject("Assets/GameData/Prefabs/Attack.prefab", 2);//异步加载
//    }


//    public void OnCallBack(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
//    {
//        go = obj as GameObject;
//    }

//    void Update () {
//	    if (Input.GetKeyDown(KeyCode.A))
//	    {
//	        ObjectManger.Instance.ReleaseObject(go);
//	        go = null;
//	    }
//        else if (Input.GetKeyDown(KeyCode.S))
//	    {
//	        go = ObjectManger.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);
//        }
//        else if (Input.GetKeyDown(KeyCode.D))
//	    {
//	        ObjectManger.Instance.ReleaseObject(go,0,true);
//	        go = null;
//        }
//    }

//    void OnApplicationQuit()
//    {
//#if UNITY_EDITOR
//        ResourceManger.Instance.ClearCache();
//        Resources.UnloadUnusedAssets();//编辑器下卸载所有未使用的资源
//        Debug.Log("应用退出，清空编辑器缓存");
//#endif
//    }
//}
#endregion
#region resMgr的使用
//public class GameStart : MonoBehaviour
//{
//    private AudioClip clip = null;
//    private AudioSource source = null;

//    void Awake()
//    {

//        clip = GetComponent<AudioClip>();
//        source = GetComponent<AudioSource>();

//        GameObject.DontDestroyOnLoad(gameObject);//切场景不销毁此控制器
//        AssetBundleManger.Instance.LoadAssetBundleConfig();//初始化配置表
//        ResourceManger.Instance.Init(this);
//        ObjectManger.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceenTrs"));
//    }

//    void Start()
//    {
//        ResourceManger.Instance.PreloadRes("Assets/GameData/Sounds/senlin.mp3");//预加载

//        //ResourceManger.Instance.AsyncLoadResource("Assets/GameData/Sounds/senlin.mp3", OnCallBack, LoadResPriority.RES_HIGH);//异步加载

//        //clip = ResourceManger.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");//同步加载
//        //source.clip = clip;
//        //source.Play();
//    }

//    //回调
//    void OnCallBack(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
//    {
//        clip = obj as AudioClip;
//        source.clip = clip;
//        source.Play();
//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.A))
//        {
//            long time = System.DateTime.Now.Ticks;
//            clip = ResourceManger.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");//同步加载
//            Debug.Log("加载用时：" + (System.DateTime.Now.Ticks - time));
//            source.clip = clip;
//            source.Play();
//        }
//        else if (Input.GetKeyDown(KeyCode.S))
//        {
//            ResourceManger.Instance.ReleaseResouce(clip, true);
//            clip = null;
//            source.clip = null;
//        }
//    }

//    void OnApplicationQuit()
//    {
//#if UNITY_EDITOR
//        ResourceManger.Instance.ClearCache();
//        Resources.UnloadUnusedAssets();//编辑器下卸载所有未使用的资源
//        Debug.Log("应用退出，清空编辑器缓存");
//#endif
//    }
//}

#endregion