using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMapManger : Singleton<GameMapManger>
{
    public Action LoadSceneOverCallBack { get; set; }//场景加载结束回调
    public Action LoadSceneEnterCallBack { get; set; }//场景加载开始回调

    public string CurrentMapName { get; set; }//场景名称
    public static int LoadingProgress = 0;//场景加载进度条

    public bool AlreadyLoadScene { get; set; }//场景加载是否完成
    
    private MonoBehaviour m_Mono;//协程脚本

    /// <summary>
    /// 初始化场景管理器
    /// </summary>
    /// <param name="mono"></param>
    public void Init(MonoBehaviour mono)
    {
        m_Mono = mono;
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="name">场景名称</param>
    public void LoadScene(string name)
    {
        LoadingProgress = 0;
        m_Mono.StartCoroutine(LoadSceneAsync(name));
        UIManager.Instance.PopUpWnd(ConStr.LOADPANEL, true, name);
    }
    /// <summary>
    /// 场景设置
    /// </summary>
    /// <param name="name"></param>
    protected void SetSceneSetting(string name)
    {
        //设置各种场景环境，可以根据配置表走
    }

    /// <summary>
    /// 场景异步加载
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadSceneAsync(string name)
    {
        AlreadyLoadScene = false;
        ClearCache();//跳场景前清理缓存
        //先load一个空场景防止内存泄漏
        AsyncOperation unLoadScene = SceneManager.LoadSceneAsync(ConStr.EMPTYSCENE, LoadSceneMode.Single);
        //等加载完成
        while (unLoadScene != null && !unLoadScene.isDone)
            yield return new WaitForEndOfFrame();
        LoadingProgress = 0;
        int targetProgrees = 0;//目标进度条
        AsyncOperation aysncScene = SceneManager.LoadSceneAsync(name);
        if (aysncScene != null && !aysncScene.isDone)
        {
            //场景加载前回调函数
            if (LoadSceneEnterCallBack != null)
                LoadSceneEnterCallBack();
            aysncScene.allowSceneActivation = false;//是否直接显示
            while (aysncScene.progress < 0.9f)
            {
                //前百分90无需处理unity场景加载，可以平滑过度
                targetProgrees = (int)aysncScene.progress * 100;
                yield return new WaitForEndOfFrame();
                //平滑过度
                while (LoadingProgress < targetProgrees)
                {
                    ++LoadingProgress;
                    yield return new WaitForEndOfFrame();
                }
            }
            //处理剩余百分10，因为unity加载到还剩0.2时会很卡顿进度条不平滑
            CurrentMapName = name;
            targetProgrees = 100;
            SetSceneSetting(name);
            while (LoadingProgress < targetProgrees - 2)
            {
                ++LoadingProgress;
                yield return new WaitForEndOfFrame();
            }

            LoadingProgress = 100;
            AlreadyLoadScene = true;
            aysncScene.allowSceneActivation = true;//是否直接显示
        }
        //场景加载完成后回调函数
        if (LoadSceneOverCallBack != null)
            LoadSceneOverCallBack();
        yield return null;
    }

    /// <summary>
    /// 清理缓存
    /// </summary>
    protected void ClearCache()
    {
        ObjectManger.Instance.ClearCache();
        ResourceManger.Instance.ClearCache();
    }
}

