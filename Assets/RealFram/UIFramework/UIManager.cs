using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 消息事件在这里添加
/// </summary>
public enum UIMsgID
{
    NONE = 0,
}

public class UIManager : Singleton<UIManager>
{
    private RectTransform m_UIRoot;//UI节点
    private RectTransform m_WndRoot;//窗口节点
    private Camera m_UICamera;//UI相机
    private EventSystem m_EventSystem;//事件系统
    private float m_CanvasRate = 0;//屏幕的宽高比

    private string UIPREFABPATH = "Assets/GameData/Prefabs/UGUI/Panel/";
    private Dictionary<string,Window> m_WindowDic = new Dictionary<string, Window>();//窗口缓存
    private Dictionary<string,Type> m_RegisterDic = new Dictionary<string, Type>();//注册的窗口名对应的窗口类型

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="m_UIRoot"></param>
    /// <param name="m_WndRoot"></param>
    /// <param name="m_UICamera"></param>
    /// <param name="m_EventSystem"></param>
    public void Init(RectTransform m_UIRoot, RectTransform m_WndRoot, Camera m_UICamera, EventSystem m_EventSystem)
    {
        this.m_UIRoot = m_UIRoot;
        this.m_WndRoot = m_WndRoot;
        this.m_UICamera = m_UICamera;
        this.m_EventSystem = m_EventSystem;

        m_CanvasRate = Screen.height /( this.m_UICamera.orthographicSize * 2 );//计算屏幕宽高比
    }
    /// <summary>
    /// 窗口注册方法
    /// </summary>
    /// <typeparam name="T">窗口泛型类</typeparam>
    /// <param name="name">窗口名</param>
    public void Register<T>(string name) where T : Window
    {
        m_RegisterDic[name] = typeof(T);
    }

    /// <summary>
    /// 根据窗口名获得窗口对象
    /// </summary>
    /// <typeparam name="T">窗口类型</typeparam>
    /// <param name="name">窗口名</param>
    /// <returns></returns>
    public T FindWndByName<T>(string name)where T:Window
    {
        Window wnd = null;
        if (m_WindowDic.TryGetValue(name, out wnd) && wnd != null)
        {
            return (T)wnd;
        }

        return null;
    }
    /// <summary>
    /// 置顶指定窗口
    /// </summary>
    /// <param name="wndName"></param>
    /// <param name="bTop"></param>
    /// <param name="paramList"></param>
    /// <returns></returns>
    public Window PopUpWnd(string wndName,bool bTop, params object[] paramList)
    {
        Window wnd = FindWndByName<Window>(wndName);
        if (wnd == null)
        {
            Type type = null;
            if (m_RegisterDic.TryGetValue(wndName, out type) && type != null)
            {
                wnd = System.Activator.CreateInstance(type) as Window;
            }
            else
            {
                Debug.Log("找不到对应的窗口脚本，窗口名为：" + wndName);
            }
            //拼接字符串耗gc，换种方式
            StringBuilder sb = new StringBuilder();
            if (wndName.EndsWith(".prefab"))
                sb.Append(UIPREFABPATH).Append(wndName);
            else
                sb.Append(UIPREFABPATH).Append(wndName).Append(".prefab");
            GameObject wndObj = ObjectManger.Instance.InstantiateObject(sb.ToString(), false, false);
            if (wndObj == null)
            {
                Debug.Log("窗口Prefab创建失败" + wndName);
                return null;
            }
            if (!m_WindowDic.ContainsKey(wndName))
                m_WindowDic.Add(wndName,wnd);
            //初始化obj
            wnd.GameObject = wndObj;
            wnd.Transform = wndObj.transform;
            wnd.Name = wndName;

            wnd.Awake(paramList);
            wndObj.transform.SetParent(m_WndRoot,false);

            if (bTop)
                wndObj.transform.SetAsLastSibling();

            wnd.OnShow(paramList);
        }
        else
        {
            ShowWindow(wndName,bTop,paramList);
        }
        return wnd;
    }
    /// <summary>
    /// 关闭指定窗口
    /// </summary>
    /// <param name="wndName">窗口名</param>
    /// <param name="destory">是否彻底销毁</param>
    public void CloseWindow(string wndName, bool destory = false)
    {
        Window wnd = FindWndByName<Window>(wndName);
        CloseWindow(wnd, destory);
    }
    /// <summary>
    /// /关闭指定窗口
    /// </summary>
    /// <param name="wnd">窗体</param>
    /// <param name="destory">是否彻底销毁</param>
    public void CloseWindow(Window wnd, bool destory = false)
    {
        if (wnd != null)
        {
            wnd.OnDisable();
            wnd.OnClose();
            if (m_WindowDic.ContainsKey(wnd.Name))
                m_WindowDic.Remove(wnd.Name);
            if (destory)
            {
                ObjectManger.Instance.ReleaseObject(wnd.GameObject,0,true);
            }
            else
            {
                ObjectManger.Instance.ReleaseObject(wnd.GameObject,recycleParent:false);//回收到回收节点会导致ui重绘，耗gc
            }

            wnd.GameObject = null;
            wnd.Transform = null;
            wnd = null;
        }
    }
    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    public void CloseAllWindow()
    {
        List<Window> tempList = new List<Window>(m_WindowDic.Values);
        for (int i = 0; i < tempList.Count; i++)
        {
            CloseWindow(tempList[i].Name);
        }
    }

    /// <summary>
    /// 切换到唯一窗口
    /// </summary>
    /// <param name="wndName"></param>
    /// <param name="bTop"></param>
    /// <param name="paramList"></param>
    public void SwitchStateByName(string wndName, bool bTop = true, params object[] paramList)
    {
        CloseAllWindow();//关闭所有窗口
        PopUpWnd(wndName, bTop, paramList);//打开窗口
    }
    /// <summary>
    /// 隐藏窗口
    /// </summary>
    /// <param name="wndName">窗口名</param>
    public void HideWindow(string wndName)
    {
        Window wnd = FindWndByName<Window>(wndName);
        HideWindow(wnd);
    }
    /// <summary>
    /// 隐藏窗口
    /// </summary>
    /// <param name="wnd">窗口对象</param>
    public void HideWindow(Window wnd)
    {
        if (wnd != null)
        {
            wnd.OnDisable();
            wnd.GameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 调用所有界面的update方法
    /// </summary>
    public void OnUpdate()
    {
        foreach (Window wnd in m_WindowDic.Values)
        {
            if(wnd!=null)
                wnd.OnUpdate();
        }
    }

    /// <summary>
    /// 调用窗口事件
    /// </summary>
    /// <param name="wndName"></param>
    /// <param name="msgId"></param>
    /// <param name="paramList"></param>
    /// <returns></returns>
    public bool SendMessageToWindow(string wndName, UIMsgID msgId, params object[] paramList)
    {
        Window wnd = FindWndByName<Window>(wndName);
        if (wnd != null)
        {
            return wnd.OnMessage( msgId, paramList);
        }

        return false;
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="wndName">窗口名</param>
    /// <param name="bTop">是否置顶</param>
    /// <param name="paramList">参数列表</param>
    public void ShowWindow(string wndName, bool bTop = true, params object[] paramList)
    {
        Window wnd = FindWndByName<Window>(wndName);
        ShowWindow(wnd, bTop, paramList);
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="wnd">窗口对象</param>
    /// <param name="bTop">是否置顶</param>
    /// <param name="">参数列表</param>
    public void ShowWindow(Window wnd, bool bTop = true, params object[] paramList)
    {
        if (wnd != null)
        {
            if( wnd.GameObject != null && !wnd.GameObject.activeSelf) 
                wnd.GameObject.SetActive(true);
            if (wnd.Transform != null && bTop)
                wnd.Transform.SetAsLastSibling();
            wnd.OnShow(paramList);
        }
    }

}
