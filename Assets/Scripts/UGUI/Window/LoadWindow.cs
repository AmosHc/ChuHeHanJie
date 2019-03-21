using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loadWindow : Window
{
    private LoadPanel m_MainPanel;
    private string m_SceneName;

    public override void Awake(params object[] paramList)
    {
        m_MainPanel = GameObject.transform.GetComponent<LoadPanel>();
        m_SceneName = (string)paramList[0];
        GameMapManger.Instance.LoadSceneOverCallBack = LoadOtherScene;//设置场景加载完成后的回调函数
    }

    public override void OnUpdate()
    {
        //根据场景名字打开对应场景第一个页面
        if (m_MainPanel == null) return;
        m_MainPanel.LoadSlider.value = GameMapManger.LoadingProgress / 100.0f;
        m_MainPanel.ProgressText.text = string.Format("{0}%", GameMapManger.LoadingProgress);
    }
    /// <summary>
    /// 加载对应场景第一个UI
    /// </summary>
    public void LoadOtherScene()
    {
        //根据打开场景加载第一个界面
        //switch (m_SceneName)
        //{
        //    //case ConStr.MENUSCENE:
        //    //    UIManager.Instance.OpenWnd(ConStr.MENUPANEL,true);
        //    //    break;
        //    default:
        //        return;
        //}
        UIManager.Instance.CloseWindow(ConStr.LOADPANEL);
    }
}
